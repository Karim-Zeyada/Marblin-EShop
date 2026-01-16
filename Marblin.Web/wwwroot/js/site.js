
$(document).ready(function () {
    // Navbar Glass Effect
    var navbar = $("#mainNavbar");
    var isHome = navbar.data("is-home") === true;

    $(window).scroll(function () {
        var scroll = $(window).scrollTop();

        if (scroll > 50) {
            navbar.addClass("glass-dark");
        } else {
            if (isHome) {
                navbar.removeClass("glass-dark");
            }
        }
    });

    // Initial check (in case user reloads halfway down the page)
    if ($(window).scrollTop() > 50) {
        navbar.addClass("glass-dark");
    }

    // Cart Badge Animation (Simple bounce when updated - placeholder for future logic)
    function animateCartBadge() {
        var badge = $("#cartBadge");
        badge.addClass("animate__animated animate__bounceIn");
        setTimeout(function() {
            badge.removeClass("animate__animated animate__bounceIn");
        }, 1000);
    }
});
