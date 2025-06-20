# ECommerce AuthServer - Tailwind CSS Kurulumu

Bu proje Tailwind CSS kullanmaktadır. Aşağıdaki adımları takip ederek geliştirme ortamını hazırlayabilirsiniz.

## Gereksinimler

- Bun (v1.0 veya üzeri)

## Kurulum

1. AuthServer klasöründe bun paketlerini yükleyin:
```bash
cd src/Infrastructure/ECommerce.AuthServer
bun install
```

2. Tailwind CSS'i build edin:
```bash
# Development için (watch mode)
bun run build-css

# Production için (minified)
bun run build-css-prod
```

## Geliştirme

Development sürecinde Tailwind CSS'in watch mode'unda çalışması için:

```bash
bun run build-css
```

Bu komut `wwwroot/css/input.css` dosyasını izleyerek değişiklikleri otomatik olarak `wwwroot/css/tailwind.css` dosyasına derleyecektir.

## Dosya Yapısı

- `package.json` - Node.js bağımlılıkları
- `tailwind.config.js` - Tailwind CSS konfigürasyonu
- `wwwroot/css/input.css` - Tailwind CSS input dosyası
- `wwwroot/css/tailwind.css` - Build edilmiş CSS dosyası (gitignore'da)

## Özel CSS Sınıfları

Proje için özel tanımlanmış CSS sınıfları:

- `.btn-primary` - Ana buton stili
- `.btn-secondary` - İkincil buton stili
- `.input-field` - Form input stili
- `.card` - Kart container stili
- `.auth-container` - Auth sayfaları container'ı
- `.auth-card` - Auth sayfaları kart stili

## Renk Paleti

- Primary: Mavi tonları (#3b82f6 - #1e3a8a)
- Gray: Gri tonları (#f9fafb - #111827)

Tüm renkler `tailwind.config.js` dosyasında tanımlanmıştır.

## Docker Kullanımı

Proje Docker container olarak çalıştırıldığında Tailwind CSS otomatik olarak build edilir. 

### Geliştirme Ortamında

Development ortamında watch mode için:

```bash
# Ana dizinden (compose.yaml'ın bulunduğu klasör)
docker-compose --profile dev up ecommerce.authserver.tailwind

# Veya AuthServer klasöründen direkt olarak
cd src/Infrastructure/ECommerce.AuthServer
./watch-tailwind.sh
```

### Production Build

Container build edilirken Tailwind CSS otomatik olarak production için optimize edilmiş şekilde build edilir. 