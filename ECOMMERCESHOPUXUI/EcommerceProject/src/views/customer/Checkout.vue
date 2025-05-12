<script setup>
</script>
<template>
  <div>
    <!-- Checkout Section Begin -->
    <section class="checkout spad">
      <div class="container">
        <form action="#" class="checkout__form">
          <div class="row">
            <div class="col-lg-8">
              <h5>Billing detail</h5>
              <div class="row">
                <div class="col-lg-12">
                  <div class="checkout__form__input">
                    <p>Họ và tên <span>*</span></p>
                    <input type="text" />
                  </div>
                  <div class="checkout__form__input">
                    <p>Country <span>*</span></p>
                    <input type="text" />
                  </div>
                  <div class="checkout__form__input">
                    <p>Address <span>*</span></p>
                    <input type="text" placeholder="Street Address" />
                  </div>
                </div>
                <div class="checkout__form__input">
                  <p>Phone <span>*</span></p>
                  <input type="text" />
                </div>
                <div class="checkout__form__input">
                  <p>Mô tả</p>
                  <textarea style="width: 100%; border: 1px solid #e1e1e1" />
                </div>
              </div>
            </div>
            <div class="col-lg-4">
              <div class="checkout__order">
                <h5>Your order</h5>
                <div class="checkout__order__product">
                  <ul>
                    <li>
                      <span class="top__text">Product</span>
                      <span class="top__text__right">Total</span>
                    </li>
                    <li>01. Chain buck bag <span>$ 300.0</span></li>
                    <li>
                      02. Zip-pockets pebbled<br />
                      tote briefcase <span>$ 170.0</span>
                    </li>
                    <li>03. Black jean <span>$ 170.0</span></li>
                    <li>04. Cotton shirt <span>$ 110.0</span></li>
                  </ul>
                </div>
                <div class="checkout__order__total">
                  <ul>
                    <li>Subtotal <span>$ 750.0</span></li>
                    <li>Total <span>$ 750.0</span></li>
                  </ul>
                </div>
                <div class="col-lg-12" style="margin-bottom: 30px;">
                  <div class="discount__content">
                    <h6>Nhập mã coupon (nếu có)</h6>
                    <div style="display: flex; gap: 10px">
                      <input
                        type="text"
                        placeholder="Nhập mã coupon"
                        style="flex: 1; min-width: 0; border: 1px solid #e1e1e1"
                      />
                      <button type="submit" class="site-btn" style="white-space: nowrap">
                        Apply
                      </button>
                    </div>
                  </div>
                </div>

                <div class="checkout__order__widget">
                  <label for="check-payment">
                    COD
                    <input type="checkbox" id="check-payment" />
                    <span class="checkmark"></span>
                  </label>
                  <label for="paypal">
                    VNPAY
                    <input type="checkbox" id="paypal" />
                    <span class="checkmark"></span>
                  </label>
                </div>
                <button type="submit" class="site-btn">Place oder</button>
              </div>
            </div>
          </div>
        </form>
      </div>
    </section>
    <!-- Checkout Section End -->
  </div>
</template>


<style scoped>
.discount__content > div {
  display: flex;
  gap: 10px;
  align-items: center;
}

.discount__content input {
  flex: 2;
  min-width: 0;
}

.discount__content .site-btn {
  flex: 0 0 auto;
  padding: 8px 16px; /* tuỳ chỉnh độ lớn nút */
  white-space: nowrap;
  width: auto !important;
  display: inline-block !important;
}
.discount__content > div {
  display: flex;
  gap: 10px;
  align-items: center;
}

.discount__content input {
  flex: 2;
  min-width: 0;
  border-radius: 8px; /* Bo góc mượt */
  padding: 8px 12px;
  border: 1px solid #ccc;
}

.discount__content .site-btn {
  flex: 0 0 auto;
  padding: 8px 16px;
  white-space: nowrap;
  border-radius: 8px; /* Bo góc cho nút */
}

</style>