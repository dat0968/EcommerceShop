<template>
  <div>
    <!-- Shop Cart Section Begin -->
    <section class="shop-cart spad">
      <div class="container">
        <div class="row">
          <div class="col-lg-9">
            <div class="shop__cart__table">
              <table>
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Price</th>
                    <th>Quantity</th>
                    <th>Total</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td class="cart__product__item">
                      <img src="@/assets/Customer/img/shop-cart/cp-1.jpg" alt="" />
                      <div class="cart__product__item__title">
                        <h6>Chain bucket bag</h6>
                      </div>
                    </td>
                    <td class="cart__price">$ 150.0</td>
                    <td class="cart__quantity">
                      <div class="pro-qty">
                        <input type="text" value="1" />
                      </div>
                    </td>
                    <td class="cart__total">$ 300.0</td>
                    <td class="cart__close"><span class="icon_close"></span></td>
                  </tr>
                  <tr>
                    <td class="cart__product__item">
                      <img src="@/assets/Customer/img/shop-cart/cp-2.jpg" alt="" />
                      <div class="cart__product__item__title">
                        <h6>Zip-pockets pebbled tote briefcase</h6>
                        
                      </div>
                    </td>
                    <td class="cart__price">$ 170.0</td>
                    <td class="cart__quantity">
                      <div class="pro-qty">
                        <input type="text" value="1" />
                      </div>
                    </td>
                    <td class="cart__total">$ 170.0</td>
                    <td class="cart__close"><span class="icon_close"></span></td>
                  </tr>
                  <tr>
                    <td class="cart__product__item">
                      <img src="@/assets/Customer/img/shop-cart/cp-3.jpg" alt="" />
                      <div class="cart__product__item__title">
                        <h6>Black jean</h6>
                        
                      </div>
                    </td>
                    <td class="cart__price">$ 85.0</td>
                    <td class="cart__quantity">
                      <div class="pro-qty">
                        <input type="text" value="1" />
                      </div>
                    </td>
                    <td class="cart__total">$ 170.0</td>
                    <td class="cart__close"><span class="icon_close"></span></td>
                  </tr>
                  <tr>
                    <td class="cart__product__item">
                      <img src="@/assets/Customer/img/shop-cart/cp-4.jpg" alt="" />
                      <div class="cart__product__item__title">
                        <h6>Cotton Shirt</h6>
                      </div>
                    </td>
                    <td class="cart__price">$ 55.0</td>
                    <td class="cart__quantity">
                      <div class="pro-qty">
                        <input type="text" value="1" />
                      </div>
                    </td>
                    <td class="cart__total">$ 110.0</td>
                    <td class="cart__close"><span class="icon_close"></span></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div class="col-lg-3">
            <div class="cart__total__procced">
              <h6>Cart total</h6>
              <ul>
                <li>Subtotal <span>$ 750.0</span></li>
                <li>Total <span>$ 750.0</span></li>
              </ul>
              <a href="#" class="primary-btn">Đặt hàng</a>
            </div>
          </div>
        </div>       
      </div>
    </section>
  </div>
  <!-- Shop Cart Section End -->
</template>

<script>
export default {}
</script>

<style>
</style>