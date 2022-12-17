import { defineConfig } from 'astro/config';

// https://astro.build/config
import compress from "astro-compress";

// https://astro.build/config
import sitemap from "@astrojs/sitemap";

// https://astro.build/config
export default defineConfig({
  site: 'https://meterialyou.vercel.app',
  integrations: [compress(), sitemap()]
});