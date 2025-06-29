// Lấy tất cả ảnh có class lazy
const lazyImages = document.querySelectorAll('img.lazy')

const defaultLazyImg = 'lazyImg.jpg'

// Hàm kiểm tra ảnh có trong viewport không
function lazyLoad() {
  lazyImages.forEach((img) => {
    if (
      img.getBoundingClientRect().top < window.innerHeight &&
      img.getAttribute('src') == defaultLazyImg
    ) {
      img.src = img.dataset.src // Set src from data-src
      img.classList.remove('lazy') // Delete class lazy
    }
  })
}

export default function addLazyLoadImages() {
  // Call api when client scroll or refresh page
  window.addEventListener('scroll', lazyLoad)
  window.addEventListener('load', lazyLoad)
}
