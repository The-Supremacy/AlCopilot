/** @type {import('tailwindcss').Config} */
const preset = {
  theme: {
    extend: {
      colors: {
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        card: 'hsl(var(--card))',
        'card-foreground': 'hsl(var(--card-foreground))',
        popover: 'hsl(var(--popover))',
        'popover-foreground': 'hsl(var(--popover-foreground))',
        primary: 'hsl(var(--primary))',
        'primary-foreground': 'hsl(var(--primary-foreground))',
        secondary: 'hsl(var(--secondary))',
        'secondary-foreground': 'hsl(var(--secondary-foreground))',
        muted: 'hsl(var(--muted))',
        'muted-foreground': 'hsl(var(--muted-foreground))',
        accent: 'hsl(var(--accent))',
        'accent-foreground': 'hsl(var(--accent-foreground))',
        destructive: 'hsl(var(--destructive))',
        'destructive-foreground': 'hsl(var(--destructive-foreground))',
        'destructive-muted': 'hsl(var(--destructive-muted))',
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        shell: 'hsl(var(--shell))',
        'shell-foreground': 'hsl(var(--shell-foreground))',
        success: 'hsl(var(--success))',
        'success-foreground': 'hsl(var(--success-foreground))',
        'success-muted': 'hsl(var(--success-muted))',
        warning: 'hsl(var(--warning))',
        'warning-foreground': 'hsl(var(--warning-foreground))',
        'warning-muted': 'hsl(var(--warning-muted))',
        info: 'hsl(var(--info))',
        'info-foreground': 'hsl(var(--info-foreground))',
        'info-muted': 'hsl(var(--info-muted))',
        brand: {
          ink: 'hsl(var(--brand-ink))',
          parchment: 'hsl(var(--brand-parchment))',
          ivory: 'hsl(var(--brand-ivory))',
          copper: 'hsl(var(--brand-copper))',
          'copper-deep': 'hsl(var(--brand-copper-deep))',
          malt: 'hsl(var(--brand-malt))',
          glass: 'hsl(var(--brand-glass))',
          mineral: 'hsl(var(--brand-mineral))',
        },
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
      fontFamily: {
        sans: ['var(--font-sans)'],
        display: ['var(--font-display)'],
      },
      boxShadow: {
        soft: '0 24px 64px rgba(15, 23, 42, 0.12)',
      },
      backgroundImage: {
        'portal-glow':
          'radial-gradient(circle at top left, hsl(var(--primary) / 0.24), transparent 28%), radial-gradient(circle at bottom right, hsl(var(--brand-glass) / 0.18), transparent 32%)',
      },
    },
  },
};

export default preset;
