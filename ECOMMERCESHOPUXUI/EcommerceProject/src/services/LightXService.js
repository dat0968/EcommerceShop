import Swal from 'sweetalert2'
import * as axiosConfig from '@/utils/axiosClient'

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

  async uploadImageToCloudinary(file) {
    console.log('[DEBUG] 1. Preparing to upload user image to backend.', { file });
    const formData = new FormData();
    const extension = file.type.split('/')[1] || 'jpg';
    const fileName = `user-model-upload.${extension}`;
    formData.append('file', file, fileName);

    console.log('[DEBUG] 2. Sending FormData to /api/TryOn/UploadImage.', { formDataContent: { name: fileName, type: file.type, size: file.size } });
    
    const response = await axiosConfig.postToApi('/TryOn/UploadImage', formData, { headers: { 'Content-Type': 'multipart/form-data' } });
    
    console.log('[DEBUG] 3. Received response from backend.', { response });

    // IMPORTANT: Check for success AND the actual URL
    if (!response.success || !response.data?.imageUrl) {
      console.error('[DEBUG] Backend indicated success, but imageUrl is missing.', response);
      throw new Error(response.message || 'Không thể lấy URL hình ảnh từ máy chủ sau khi upload.');
    }
    
    console.log('[DEBUG] 4. Backend upload successful. Public URL:', response.data.imageUrl);
    return response.data.imageUrl;
  }

  async uploadImageFromUrlToCloudinary(imageUrl) {
    const response = await axiosConfig.postToApi('/TryOn/UploadFromUrl', { imageUrl });
    if (!response.success) throw new Error(response.message || 'Không thể tải ảnh từ URL lên Cloudinary.');
    return response.data.imageUrl;
  }

  async analyzeImageWithGemini(resultImageUrl, productsData) {
    const response = await axiosConfig.postToApi('/TryOn/AnalyzeImage', { resultImageUrl, productsData });
    if (!response.success) throw new Error(response.message || 'Phân tích hình ảnh bằng Gemini AI thất bại.');
    return response.data;
  }

  async processWithLightX(apiKey, modelImageUrl, products) {
    console.log('[DEBUG] processWithLightX: Starting...', { modelImageUrl, products });
    try {
      // Step 1: Ensure model image is publicly accessible
      let finalModelImageUrl = modelImageUrl;
      if (modelImageUrl.startsWith('data:')) { // If it's a data URL (user uploaded)
        console.log('[DEBUG] processWithLightX: User model is a data URL. Converting to blob.');
        const blob = dataURLtoBlob(modelImageUrl);
        if (!blob) {
          console.error('[DEBUG] processWithLightX: dataURLtoBlob failed.');
          throw new Error('Không thể chuyển đổi ảnh người mẫu sang định dạng có thể xử lý.');
        }
        finalModelImageUrl = await this.uploadImageToCloudinary(blob);
      } else if (modelImageUrl.includes('localhost')) { // If it's a localhost URL
        console.log('[DEBUG] processWithLightX: Model is a localhost URL. Uploading to Cloudinary.');
        finalModelImageUrl = await this.uploadImageFromUrlToCloudinary(modelImageUrl);
      }
      console.log('[DEBUG] processWithLightX: Final model public URL:', finalModelImageUrl);


      // Step 2: Ensure product images are publicly accessible
      const productPublicUrls = [];
      console.log('[DEBUG] processWithLightX: Processing product images...');
      for (const item of products) {
        let imgUrl = item.image || (item.products && item.products[0]?.image);
        if (imgUrl) {
          if (imgUrl.includes('localhost')) {
            console.log('[DEBUG] processWithLightX: Product image is a localhost URL. Uploading:', imgUrl);
            imgUrl = await this.uploadImageFromUrlToCloudinary(imgUrl);
          }
          productPublicUrls.push({ url: imgUrl, category: this.getClothingCategory(item.name) });
        }
      }
      console.log('[DEBUG] processWithLightX: Final product public URLs:', productPublicUrls);


      const topImageUrl = productPublicUrls.find(p => p.category === 'top')?.url;
      const bottomImageUrl = productPublicUrls.find(p => p.category === 'bottom')?.url;

      if (!topImageUrl && !bottomImageUrl) {
        throw new Error('Không có sản phẩm nào phù hợp (áo hoặc quần) để thử đồ.');
      }

      // Step 3: Start the job with appropriate parameters
      const payload = { apiKey, finalModelImageUrl, topImageUrl, bottomImageUrl };
      console.log('[DEBUG] processWithLightX: Starting LightX job with payload:', payload);
      const orderId = await this.startLightXJob(apiKey, finalModelImageUrl, topImageUrl, bottomImageUrl);
      console.log('[DEBUG] processWithLightX: LightX job started. Order ID:', orderId);


      // Step 4: Poll for the result
      console.log('[DEBUG] processWithLightX: Polling for LightX result...');
      const resultUrl = await this.pollLightXJob(apiKey, orderId);
      console.log('[DEBUG] processWithLightX: LightX job finished. Result URL:', resultUrl);
      return resultUrl;

    } catch (error) {
      console.error('[DEBUG] processWithLightX: An error occurred in the process.', error); // ! ERROR
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
          'Không thể xử lý ảnh bằng AI. Điều này có thể do ảnh người mẫu hoặc ảnh sản phẩm không phù hợp (ví dụ: độ phân giải thấp, khuôn mặt không rõ ràng, hoặc định dạng không được hỗ trợ). Vui lòng thử sử dụng một ảnh khác.';
      } else if (userMessage.includes('timed out')) {
        userMessage = 'Quá trình xử lý mất quá nhiều thời gian. Vui lòng thử lại sau.';
      }

      Swal.fire({
        icon: 'error',
        title: 'Lỗi xử lý ảnh từ LightX',
        text: userMessage,
      });
      throw error; // Re-throw to be caught by the calling function
    }
  }

  async performTryOnAndAnalysis(apiKey, modelImageUrl, productsData) {
    // Step 1: Process with LightX to get the try-on image URL.
    // This step is critical. If it fails, we throw the error because we can't proceed.
    const tryOnImageUrl = await this.processWithLightX(apiKey, modelImageUrl, productsData);

    // Step 2: Analyze the try-on image with Gemini AI.
    // This step is non-critical. If it fails, we return the image URL with a null result.
    let analysisResult = null;
    try {
      analysisResult = await this.analyzeImageWithGemini(tryOnImageUrl, productsData);
    } catch (analysisError) {
      console.error('Gemini AI analysis failed, but the try-on image was generated successfully.', analysisError);
      // We don't re-throw the error. The calling function will receive analysisResult = null.
    }

    return { tryOnImageUrl, analysisResult };
  }
}

export default new LightXService();