/** @type {import('tailwindcss').Config} */
export default {
    content: ["../**/*.{razor,html,cshtml,cs}", "./**/*.js"],
    darkMode: 'selector',
    theme: {
        fontFamily: {
            'sans': ['fontin', 'ui-sans-serif', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'Arial', 'Noto Sans', 'sans-serif'],
            'caps': ['fontin-smallcaps', 'fontin', 'ui-sans-serif', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'Arial', 'Noto Sans', 'sans-serif'],
        },
        fontSize: {
            xs: '0.625rem',
            sm: '0.75rem',
            base: '0.875rem',
            lg: '1rem',
            xl: '1.125rem',
            '2xl': '1.5rem',
            '3xl': '1.875rem'
        }
    }
}
