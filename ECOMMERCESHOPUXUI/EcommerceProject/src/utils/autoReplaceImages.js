import defaultImage from '@/assets/default/default.webp' // Import ảnh

const replaceBrokenImages = () => {
  const checkAndReplace = (img) => {
    if (!img.dataset.checked) {
      img.dataset.checked = 'true'
      img.onerror = () => {
        // Nếu đã là ảnh mặc định mà vẫn lỗi thì không thay nữa
        if (img.src !== defaultImage) {
          img.src = defaultImage
        } else {
          // Đã thử thay ảnh mặc định mà vẫn lỗi, bỏ qua không thay nữa
          img.onerror = null
        }
      }
    }
  }

  document.querySelectorAll('img').forEach(checkAndReplace)

  const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      mutation.addedNodes.forEach((node) => {
        if (node.nodeType === 1) {
          if (node.tagName === 'IMG') {
            checkAndReplace(node)
          } else {
            node.querySelectorAll?.('img').forEach(checkAndReplace)
          }
        }
      })
    })
  })

  observer.observe(document.body, {
    childList: true,
    subtree: true,
  })
}

export default replaceBrokenImages
