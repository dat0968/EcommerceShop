import Swal from 'sweetalert2'

// Helper function to convert dataURL to Blob
function dataURLtoBlob(dataurl) {
  if (!dataurl || typeof dataurl !== 'string') {
    console.error('dataURLtoBlob: Invalid dataurl input', dataurl)
    return null
  }
  if (!dataurl.startsWith('data:')) {
    console.error('dataURLtoBlob: Input is not a data URL', dataurl)
    return null
  }
  const arr = dataurl.split(',')
  if (arr.length < 2) {
    console.error('dataURLtoBlob: Malformed dataurl, missing comma', dataurl)
    return null
  }
  const mimeMatch = arr[0].match(/:(.*?);/)
  if (!mimeMatch || !mimeMatch[1]) {
    console.error('dataURLtoBlob: Could not extract mime type', arr[0])
    return null
  }
  const mime = mimeMatch[1]
  let bstr
  try {
    bstr = atob(arr[1])
  } catch (e) {
    console.error('dataURLtoBlob: Failed to decode base64', e, arr[1])
    return null
  }
  const n = bstr.length
  const u8arr = new Uint8Array(n)
  for (let i = 0; i < n; i++) {
    u8arr[i] = bstr.charCodeAt(i)
  }
  return new Blob([u8arr], { type: mime })
}

// Helper function to load image
function loadImage(url) {
  return new Promise((resolve, reject) => {
    const img = new window.Image()
    img.crossOrigin = 'Anonymous' // Request CORS
    img.onload = () => resolve(img)
    img.onerror = (e) => {
      console.error('Error loading image:', url, e)
      reject(new Error(`Failed to load image from ${url}. Check URL and CORS settings.`))
    }
    img.src = url
  })
}

// Helper function to load image as data URL
async function loadImageAsDataUrl(url) {
  const img = await loadImage(url);
  const canvas = document.createElement('canvas');
  canvas.width = img.naturalWidth;
  canvas.height = img.naturalHeight;
  const ctx = canvas.getContext('2d');
  ctx.drawImage(img, 0, 0);
  return canvas.toDataURL('image/jpeg');
}

class LightXService {
  constructor() {
    // Khởi tạo các thành phần cần thiết cho LightX
  }

  getClothingCategory(categoryName) {
    if (!categoryName) return 'unknown';
    const name = categoryName.toLowerCase();
    if (name.includes('áo') || name.includes('top')) {
      return 'top';
    }
    if (name.includes('quần') || name.includes('bottom') || name.includes('pants') || name.includes('jeans')) {
      return 'bottom';
    }
    return 'unknown';
  }

  async getLightXUploadUrl(apiKey, size) {
    const response = await fetch('https://api.lightxeditor.com/external/api/v2/uploadImageUrl', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'x-api-key': apiKey,
      },
      body: JSON.stringify({
        uploadType: 'imageUrl',
        size: size,
        contentType: 'image/jpeg',
      }),
    })
    const data = await response.json()
    if (data.statusCode !== 2000) {
      console.error('LightX getUploadUrl failed. Full response:', data)
      throw new Error('Failed to get LightX upload URL: ' + data.message)
    }
    return data.body
  }

  async uploadToLightX(uploadUrl, blob) {
    const response = await fetch(uploadUrl, {
      method: 'PUT',
      headers: { 'Content-Type': 'image/jpeg' },
      body: blob,
    })
    if (!response.ok) {
      const errorText = await response.text()
      console.error('LightX image upload failed. Full response:', errorText)
      throw new Error('Failed to upload image to LightX.')
    }
  }

  async startLightXJob(apiKey, imageUrl, topImageUrl, bottomImageUrl) {
    const payload = {
      imageUrl,
      category: "fashion", // Assuming fashion category for virtual try-on
    };

    if (topImageUrl) {
      payload.styleImageUrl = topImageUrl;
      payload.clothCategory = "top";
    }

    if (bottomImageUrl) {
      // If there's also a top, this becomes a sticker
      if (topImageUrl) {
        payload.stickerImageUrl = bottomImageUrl;
        payload.stickerCategory = "bottom";
      } else { // Otherwise, it's the main style
        payload.styleImageUrl = bottomImageUrl;
        payload.clothCategory = "bottom";
      }
    }

    const response = await fetch('https://api.lightxeditor.com/external/api/v2/aivirtualtryon', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'x-api-key': apiKey,
      },
      body: JSON.stringify(payload),
    })
    const data = await response.json()
    if (data.statusCode !== 2000) {
      console.error('LightX startJob failed. Full response:', data)
      throw new Error('Failed to start LightX job: ' + data.message)
    }
    return data.body.orderId
  }

  async pollLightXJob(apiKey, orderId) {
    const maxRetries = 10 // Tăng số lần thử lại
    const delay = 5000 // Tăng thời gian chờ lên 5 giây

    for (let i = 0; i < maxRetries; i++) {
      await new Promise((resolve) => setTimeout(resolve, delay))
      const response = await fetch('https://api.lightxeditor.com/external/api/v2/order-status', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-api-key': apiKey,
        },
        body: JSON.stringify({ orderId }),
      })
      const data = await response.json()

      if (data.statusCode !== 2000 || !data.body) {
        console.error('LightX pollJob failed or returned unexpected data. Full response:', data)
        throw new Error(`LightX job status check failed: ${data.message || 'No response body'}`)
      }

      if (data.body.status === 'active') {
        return data.body.output
      }
      if (data.body.status === 'failed') {
        console.error('LightX job failed. Full response:', data)
        throw new Error('LightX job failed.')
      }
    }
    throw new Error('LightX job timed out after several retries.')
  }

  async processWithLightX(apiKey, modelImageUrl, products) {
    try {
      // Step 1: Load all images as data URLs to be sent to LightX
      const modelDataUrl = await loadImageAsDataUrl(modelImageUrl);
      const productDataUrls = [];
      for(const product of products) {
          const imgUrl = product.image || (product.products && product.products[0]?.image);
          if (imgUrl) {
            productDataUrls.push(await loadImageAsDataUrl(imgUrl));
          }
      }

      // Step 2: Upload model image
      const modelBlob = dataURLtoBlob(modelDataUrl);
      if (!modelBlob) {
        throw new Error('Không thể chuyển đổi ảnh người mẫu sang định dạng có thể xử lý. Ảnh có thể bị hỏng.');
      }
      const modelUploadData = await this.getLightXUploadUrl(apiKey, modelBlob.size);
      await this.uploadToLightX(modelUploadData.uploadImage, modelBlob);
      const finalModelImageUrl = modelUploadData.imageUrl;

      // Step 3: Upload all product images and categorize them
      const productUrls = [];
      for (let i = 0; i < productDataUrls.length; i++) {
          const productDataUrl = productDataUrls[i];
          const productBlob = dataURLtoBlob(productDataUrl);
          if (!productBlob) {
              console.warn(`Could not process product image, skipping.`);
              continue;
          }
          const productUploadData = await this.getLightXUploadUrl(apiKey, productBlob.size);
          await this.uploadToLightX(productUploadData.uploadImage, productBlob);
          const originalProduct = products[i]; // Get the original product to access its category
          productUrls.push({
              url: productUploadData.imageUrl,
              category: this.getClothingCategory(originalProduct.name) // 'top' or 'bottom'
          });
      }

      const topImageUrl = productUrls.find(p => p.category === 'top')?.url;
      const bottomImageUrl = productUrls.find(p => p.category === 'bottom')?.url;

      if (!topImageUrl && !bottomImageUrl) {
          throw new Error('Không có sản phẩm nào phù hợp (áo hoặc quần) để thử đồ.');
      }

      // Step 4: Start the job with appropriate parameters
      const orderId = await this.startLightXJob(apiKey, finalModelImageUrl, topImageUrl, bottomImageUrl);

      // Step 5: Poll for the result
      const resultUrl = await this.pollLightXJob(apiKey, orderId);
      return resultUrl;

    } catch (error) {
      console.error('Error processing with LightX API:', error);
      let userMessage = 'Đã có lỗi không xác định xảy ra.'; // Default generic message

      if (error instanceof Error) {
          userMessage = error.message;
      } else if (typeof error === 'string') {
          userMessage = error;
      } else if (error && error.message) {
          userMessage = error.message;
      }

      if (userMessage.includes('5044')) {
        userMessage =
          'Không thể xử lý ảnh bằng AI. Điều này có thể do ảnh người mẫu hoặc ảnh sản phẩm không phù hợp (ví dụ: độ phân giải thấp, khuôn mặt không rõ ràng, hoặc định dạng không được hỗ trợ). Vui lòng thử sử dụng một ảnh khác.'
      } else if (userMessage.includes('timed out')) {
        userMessage = 'Quá trình xử lý mất quá nhiều thời gian. Vui lòng thử lại sau.'
      }

      Swal.fire({
        icon: 'error',
        title: 'Lỗi xử lý ảnh từ LightX',
        text: userMessage,
      });
      throw error; // Re-throw to be caught by the calling function
    }
  }
}

export default new LightXService();
