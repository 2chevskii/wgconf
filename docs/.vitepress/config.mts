import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "WgConf Documentation",
  description: "Documentation for WgConf and WgConf.Amnezia packages",
  base: "/wgconf/",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'Reference', link: '/reference/core-types' },
      { text: 'Formats', link: '/formats/wireguard' }
    ],

    sidebar: {
      '/guide/': [
        {
          text: 'Guide',
          items: [
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'Configuration Model', link: '/guide/configuration-model' },
            { text: 'Parsing and Writing', link: '/guide/parsing-writing' },
            { text: 'Error Handling', link: '/guide/error-handling' }
          ]
        }
      ],
      '/reference/': [
        {
          text: 'Reference',
          items: [
            { text: 'Core Types', link: '/reference/core-types' },
            { text: 'Value Types', link: '/reference/value-types' },
            { text: 'Readers and Writers', link: '/reference/readers-writers' },
            { text: 'AmneziaWG Extension', link: '/reference/amnezia' }
          ]
        }
      ],
      '/formats/': [
        {
          text: 'Formats',
          items: [
            { text: 'WireGuard', link: '/formats/wireguard' },
            { text: 'AmneziaWG', link: '/formats/amneziawg' }
          ]
        }
      ],
      '/': [
        {
          text: 'Start',
          items: [
            { text: 'Overview', link: '/' },
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'WireGuard Format', link: '/formats/wireguard' }
          ]
        }
      ]
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/2chevskii/wgconf' }
    ]
  }
})
