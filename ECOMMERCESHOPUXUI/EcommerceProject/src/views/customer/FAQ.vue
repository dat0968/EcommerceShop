<template>
  <div class="container py-5">
    <h1 class="mb-4">Các Câu Hỏi Thường Gặp (FAQ)</h1>
    <p class="lead mb-4">Dưới đây là một số câu hỏi thường gặp về EcommerceShop. Nếu bạn không tìm thấy câu trả lời mình cần, đừng ngần ngại liên hệ với chúng tôi qua chatbot hoặc trang Liên hệ.</p>
    <div class="accordion" id="faqAccordion">
      <div class="accordion-item">
        <h2 class="accordion-header" id="headingOne">
          <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseOne" aria-expanded="false" aria-controls="collapseOne">
            Làm thế nào để đặt hàng trên EcommerceShop?
          </button>
        </h2>
        <div id="collapseOne" class="accordion-collapse collapse" aria-labelledby="headingOne" data-bs-parent="#faqAccordion">
          <div class="accordion-body">
            Bạn có thể dễ dàng đặt hàng bằng cách duyệt qua các sản phẩm, thêm chúng vào giỏ hàng và tiến hành thanh toán. Chúng tôi hỗ trợ thanh toán an toàn qua VNPAY.
          </div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="headingTwo">
          <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseTwo" aria-expanded="false" aria-controls="collapseTwo">
            Làm thế nào để theo dõi trạng thái đơn hàng của tôi?
          </button>
        </h2>
        <div id="collapseTwo" class="accordion-collapse collapse" aria-labelledby="headingTwo" data-bs-parent="#faqAccordion">
          <div class="accordion-body">
            Sau khi đặt hàng thành công, bạn sẽ nhận được một email xác nhận kèm theo liên kết để theo dõi trạng thái đơn hàng của mình. Bạn cũng có thể kiểm tra trong phần "Lịch sử đơn hàng" trong tài khoản của mình.
          </div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="headingThree">
          <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseThree" aria-expanded="false" aria-controls="collapseThree">
            Chính sách đổi trả sản phẩm của EcommerceShop như thế nào?
          </button>
        </h2>
        <div id="collapseThree" class="accordion-collapse collapse" aria-labelledby="headingThree" data-bs-parent="#faqAccordion">
          <div class="accordion-body">
            Chúng tôi chấp nhận đổi trả sản phẩm trong vòng 30 ngày kể từ ngày mua hàng, với điều kiện sản phẩm còn nguyên vẹn và chưa qua sử dụng. Vui lòng tham khảo trang <a href="#" class="text-primary">Chính sách đổi trả</a> của chúng tôi để biết thêm chi tiết.
          </div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="headingFour">
          <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseFour" aria-expanded="false" aria-controls="collapseFour">
            Tôi có thể sử dụng mã giảm giá từ vòng quay may mắn như thế nào?
          </button>
        </h2>
        <div id="collapseFour" class="accordion-collapse collapse" aria-labelledby="headingFour" data-bs-parent="#faqAccordion">
          <div class="accordion-body">
            Mã giảm giá bạn nhận được từ vòng quay may mắn có thể được áp dụng tại trang thanh toán. Chỉ cần nhập mã vào ô "Mã giảm giá" và nhấn áp dụng để nhận ưu đãi.
          </div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="headingFive">
          <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseFive" aria-expanded="false" aria-controls="collapseFive">
            Làm thế nào để liên hệ với bộ phận hỗ trợ khách hàng?
          </button>
        </h2>
        <div id="collapseFive" class="accordion-collapse collapse" aria-labelledby="headingFive" data-bs-parent="#faqAccordion">
          <div class="accordion-body">
            Bạn có thể liên hệ với chúng tôi thông qua chatbot tích hợp trên trang web hoặc gửi email trực tiếp đến địa chỉ hỗ trợ của chúng tôi. Chi tiết liên hệ có tại trang <a href="#" class="text-primary">Liên hệ</a>.
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'FAQ'
}
</script>

<style scoped>
</style>