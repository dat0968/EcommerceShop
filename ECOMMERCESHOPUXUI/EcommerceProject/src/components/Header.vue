<template>
    <div>
        <!-- Offcanvas Menu Begin -->
        <div class="offcanvas-menu-overlay"></div>
        <div class="offcanvas-menu-wrapper">
            <div class="offcanvas__close">+</div>
            <ul class="offcanvas__widget">
                <li><span class="icon_search search-switch"></span></li>
                <li><a href="#"><span class="icon_heart_alt"></span>
                        <div class="tip">2</div>
                    </a></li>
                <li><a href="#"><span class="icon_bag_alt"></span>
                        <div class="tip">2</div>
                    </a></li>
            </ul>
            <div class="offcanvas__logo">
                <a href="./index.html"><img src="../assets/img/logo.png" alt=""></a>
            </div>
            <div id="mobile-menu-wrap"></div>
            <div class="offcanvas__auth">
                <a href="#">Đăng Nhập</a>
                <a href="#">Đăng Ký</a>
            </div>
        </div>
        <!-- Offcanvas Menu End -->

        <!-- Header Section Begin -->
        <header class="header">
            <div class="container-fluid">
                <div class="row">
                    <div class="col-xl-3 col-lg-2" style="width: 300px;margin-right: 50px; padding-bottom: 20px;">
                        <svg viewBox="0 0 700 250" role="img"
                            aria-label="Angel soft curvy logo with wings and animated gradient">
                            <defs>
                                <linearGradient id="start" x1="0%" y1="0%" x2="0%" y2="100%">
                                    <stop offset="20%" stop-color="#EC4E79">
                                        <animate attributeName="stop-color"
                                            values="#EC4E79; #ABA2B7; #5CCAE7; #ABA2B7; #EC4E79;" dur="6s"
                                            repeatCount="indefinite" />
                                    </stop>
                                    <stop offset="40%" stop-color="#ABA2B7">
                                        <animate attributeName="stop-color"
                                            values="#ABA2B7; #5CCAE7; #EC4E79; #5CCAE7; #ABA2B7;" dur="6s"
                                            repeatCount="indefinite" />
                                    </stop>
                                    <stop offset="55%" stop-color="#5CCAE7">
                                        <animate attributeName="stop-color"
                                            values="#5CCAE7; #ABA2B7; #EC4E79; #ABA2B7; #5CCAE7;" dur="6s"
                                            repeatCount="indefinite" />
                                    </stop>
                                </linearGradient>
                            </defs>

                            <!-- Left wing - smooth curves -->
                            <path class="wing left"
                                d="M160 130 C110 90, 90 180, 150 170 C130 150, 140 110, 160 130 Z" />
                            <path class="wing left" d="M150 140 C120 120, 110 170, 150 160 C140 140, 130 120, 150 140 Z"
                                opacity="0.5" />

                            <!-- Right wing - smooth curves -->
                            <path class="wing right"
                                d="M540 130 C590 90, 610 180, 550 170 C570 150, 560 110, 540 130 Z" />
                            <path class="wing right"
                                d="M550 140 C580 120, 590 170, 550 160 C560 140, 570 120, 550 140 Z" opacity="0.5" />

                            <!-- Angel text with soft cursive font -->
                            <text x="50%" y="60%" dominant-baseline="middle" text-anchor="middle" class="angel-text">
                                Angel
                            </text>
                        </svg>
                    </div>
                    <div class="col-xl-6 col-lg-7">
                        <nav class="header__menu">
                            <ul>
                                <li>
                                    <RouterLink to="/">Trang Chủ</RouterLink>
                                </li>
                                <li>
                                    <RouterLink to='/Shop'>Cửa Hàng</RouterLink>
                                </li>
                            </ul>
                        </nav>
                    </div>
                    <div class="col-lg-3">
                        <div class="header__right">
                            <div class="header__right__auth">
                                <a href="#">Đăng Nhập</a>
                                <a href="#">Đăng Ký</a>
                            </div>
                            <ul class="header__right__widget">
                                <li><a href="#"><span class="icon_heart_alt"></span>
                                        <div class="tip">2</div>
                                    </a></li>
                                <li><a href="#"><span class="icon_bag_alt"></span>
                                        <div class="tip">2</div>
                                    </a></li>
                            </ul>
                        </div>
                    </div>
                </div>
                <div class="canvas__open">
                    <i class="fa fa-bars"></i>
                </div>
            </div>
        </header>
        <!-- Header Section End -->
    </div>
</template>

<script>
import { RouterLink } from 'vue-router';


</script>

<style>
.header__menu {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100%;
}

.header__menu ul {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
}

.header__menu li {
    margin: 0 15px;
}

.header__menu li a {
    text-decoration: none;
    color: #333;
}
</style>