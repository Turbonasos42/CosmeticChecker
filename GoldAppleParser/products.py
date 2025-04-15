import asyncio
from playwright.async_api import async_playwright
import json

# URL для получения cookies
INITIAL_URL = "https://goldapple.ru/uhod"

# Файл с ссылками на товары
LINKS_FILE = "goldapple_links.json"

# Файл для сохранения результатов
OUTPUT_FILE = "goldapple_products.json"


async def get_product_details(page, product_url):
    try:
        # Извлекаем itemId из URL товара
        item_id = product_url.split("ru/")[1].split("-")[0]

        # Формируем URL API для деталей товара
        api_url = (
            f"https://goldapple.ru/front/api/catalog/product-card/base/v2?locale=ru"
            f"&itemId={item_id}&customerGroupId=0&cityId=0c5b2444-70a0-4932-980c-b4dc0d3f02b5"
            f"&cityDistrict=%D0%92%D0%BE%D1%81%D1%82%D0%BE%D1%87%D0%BD%D0%BE%D0%B5+%D0%94%D0%B5%D0%B3%D1%83%D0%BD%D0%B8%D0%BD%D0%BE"
            f"&geoPolygons[]=EKB-000000469&geoPolygons[]=EKB-000000466&geoPolygons[]=EKB-000000563"
            f"&geoPolygons[]=EKB-000000877&geoPolygons[]=EKB-000000441&regionId=0c5b2444-70a0-4932-980c-b4dc0d3f02b5"
        )

        # Выполняем запрос к API через evaluate()
        response_text = await page.evaluate(
            """
            async ({ apiUrl }) => {
                const xhr = new XMLHttpRequest();
                xhr.open('GET', apiUrl, false);
                xhr.setRequestHeader('Content-Type', 'application/json');
                xhr.send();
                return xhr.responseText;
            }
            """,
            {"apiUrl": api_url}
        )

        # Проверяем ответ
        if "<!DOCTYPE html>" in response_text:
            print(f"Товар {product_url}: HTML вместо JSON")
            return None

        data = json.loads(response_text)

        # Извлекаем необходимые данные
        product_data = data.get('data', {})
        attributes = product_data.get('productDescription', [])

        # Парсим атрибуты
        # Парсинг категории
        breadcrumbs = product_data.get('breadcrumbs', [])
        category = breadcrumbs[-1]['text'] if breadcrumbs and breadcrumbs[-1]['text'] != "Главная" else ""

        variants = product_data.get('variants', [])
        if variants:
            price_actual = variants[0].get('price', {}).get('actual', {}).get('amount', 0)
            price_regular = variants[0].get('price', {}).get('regular', {}).get('amount', 0)
            price = price_actual if price_actual > 0 else price_regular
        else:
            price = 0

        ingredients = ""
        skin_type = ""
        product_type = ""  # По умолчанию пустая строка

        for section in attributes:
            if section.get('type') == "Description":
                for attr in section.get('attributes', []):
                    if attr['key'] == "тип продукта":
                        product_type = attr['value']
                    elif attr['key'] == "тип кожи":
                        skin_type = attr['value']
            elif section.get('type') == "Text" and section.get('text') == "Состав":
                ingredients = section.get('content', "")

        # Формируем результат
        return {
            "url": product_url.strip().replace('\"', '').replace(',', ''),  # Убираем лишние пробелы и экранирование
            "name": product_data.get('name', ''),
            "brand": product_data.get('brand', ''),
            "category": category,
            "product_type": product_type,  # Может быть пустым, если не найден
            "ingredients": ingredients,
            "skin_type": skin_type,
            "price": price
        }

    except Exception as e:
        print(f"Ошибка при парсинге {product_url}: {e}")
        return None


async def main():
    async with async_playwright() as p:
        # Запускаем браузер
        browser = await p.chromium.launch(headless=True)
        context = await browser.new_context()
        page = await context.new_page()

        try:
            # Открываем страницу для получения cookies
            print("Открываем страницу для получения cookies...")
            await page.goto(INITIAL_URL, wait_until="networkidle", timeout=60000)

            # Читаем ссылки из файла
            with open(LINKS_FILE, 'r', encoding='utf-8') as f:
                product_urls = [line.strip() for line in f if line.strip()]

            total_products = len(product_urls)
            print(f"Найдено {total_products} ссылок на товары")

            # Парсим все товары
            semaphore = asyncio.Semaphore(5)  # Ограничение на 5 одновременных запросов
            results = []

            async def parse_and_save(url):
                async with semaphore:
                    details = await get_product_details(page, url)
                    if details:
                        results.append(details)
                        # Сохраняем по мере парсинга
                        with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
                            json.dump(results, f, ensure_ascii=False, indent=4)
                    await asyncio.sleep(0.2)  # Добавляем задержку между запросами

            # Создаем задачи
            tasks = [parse_and_save(url) for url in product_urls]
            await asyncio.gather(*tasks)

        finally:
            # Закрываем ресурсы
            await page.close()
            await context.close()
            await browser.close()


# Запуск
asyncio.run(main())