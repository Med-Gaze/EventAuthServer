(function ()
{
    window.addEventListener("load", function ()
    {
        setTimeout(function ()
        {
            // Section 01 - Set url link 
            var logo = document.getElementsByClassName('link');
            logo[0].href = "";

            // Section 02 - Set logo
            logo[0].children[0].sizes = "128x128";
            logo[0].children[0].alt = "Advance Api";
            logo[0].children[0].src = "/swagger/ui/images/logo.png";

            // Section 03 - Set 32x32 favicon
            var linkIcon = document.createElement('link');
            linkIcon.type = 'image/ico';
            linkIcon.rel = 'icon';
            linkIcon.sizes = '16x16';
            linkIcon.href = '/swagger/ui/images/favicon-16x16.ico';
            document.getElementsByTagName('head')[0].appendChild(linkIcon);

            var linkIcon32 = document.createElement('link');
            linkIcon32.type = 'image/ico';
            linkIcon32.rel = 'icon';
            linkIcon32.sizes = '32x32';
            linkIcon32.href = '/swagger/ui/images/favicon-32x32.ico';
            document.getElementsByTagName('head')[0].appendChild(linkIcon32);
        });
    });
})();