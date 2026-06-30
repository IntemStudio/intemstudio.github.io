(function () {
  var storageKey = "intem-theme";
  var root = document.documentElement;
  var button = document.getElementById("theme-toggle");

  if (!button) {
    return;
  }

  function getTheme() {
    return root.getAttribute("data-theme") === "dark" ? "dark" : "light";
  }

  function applyTheme(theme) {
    root.setAttribute("data-theme", theme);
    localStorage.setItem(storageKey, theme);

    var isDark = theme === "dark";
    button.setAttribute("aria-pressed", isDark ? "true" : "false");
    button.setAttribute(
      "aria-label",
      isDark ? "라이트 모드로 전환" : "다크 모드로 전환"
    );
  }

  button.addEventListener("click", function () {
    applyTheme(getTheme() === "dark" ? "light" : "dark");
  });

  applyTheme(getTheme());
})();
