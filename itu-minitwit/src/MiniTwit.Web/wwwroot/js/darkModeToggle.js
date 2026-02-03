document.addEventListener("DOMContentLoaded", function () {
    const toggleButton = document.getElementById("dark-mode-toggle");
    const themeImage = document.getElementById("theme-image");
    const body = document.body;
    let isDarkMode = false;

    // Check stored preference and apply
    if (localStorage.getItem("dark-mode") === "enabled") {
        body.classList.add("dark-mode");
        isDarkMode = true;
        themeImage.src = "/images/sun-line.png"
    }

    toggleButton.addEventListener("click", function () {
        body.classList.toggle("dark-mode");
        isDarkMode = !isDarkMode;
        
        if(isDarkMode) {
            themeImage.src = "/images/sun-line.png"   
        } else {
            themeImage.src = "/images/moon-line.png"
        }
        
        // Store preference in localStorage
        if (body.classList.contains("dark-mode")) {
            localStorage.setItem("dark-mode", "enabled");
        } else {
            localStorage.removeItem("dark-mode");
        }
    });
});