from playwright.sync_api import sync_playwright
import time
import json

# URL для открытия страницы и API
page_url = "https://goldapple.ru/uhod"
api_url = "https://goldapple.ru/front/api/catalog/products?locale=ru"

# Тело запроса (JSON)
payload_template = {
    "categoryId": "1000000004",
    "cityId": "0c5b2444-70a0-4932-980c-b4dc0d3f02b5",
    "cityDistrict": "Восточное Дегунино",
    "geoPolygons": [
        "EKB-000000469",
        "EKB-000000466",
        "EKB-000000563",
        "EKB-000000877",
        "EKB-000000441"
    ],
    "regionId": "0c5b2444-70a0-4932-980c-b4dc0d3f02b5"
}

all_links = []

with sync_playwright() as p:
    # Запускаем браузер
    browser = p.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    try:
        # 1. Открываем страницу /uhod для получения cookies
        print("Открываем страницу для получения cookies...")
        page.goto(page_url, wait_until="networkidle", timeout=60000)

        # 2. Получаем cookies из контекста браузера
        cookies = context.cookies()

        # 3. Устанавливаем cookies для API-запроса
        context.add_cookies(cookies)

        # 4. Выполняем POST-запрос к API через evaluate()
        print("Выполняем запрос к API...")
        payload = payload_template.copy()
        payload["pageNumber"] = 1
        response_text = page.evaluate(
            """
            async ({ apiUrl, requestData }) => {
                const xhr = new XMLHttpRequest();
                xhr.open('POST', apiUrl, false);
                xhr.setRequestHeader('Content-Type', 'application/json');
                xhr.send(JSON.stringify(requestData));
                return xhr.responseText;
            }
            """,
            {
                "apiUrl": api_url,
                "requestData": payload
            }
        )

        # Проверяем ответ
        if "<!DOCTYPE html>" in response_text:
            print("Ошибка: Сервер вернул HTML вместо JSON.")
            exit()

        data = json.loads(response_text)
        total_count = data['data']['count']
        total_pages = (total_count + 23) // 24
        print(f"Всего товаров: {total_count}, страниц: {total_pages}")

        # Парсим все страницы
        for page_number in range(1, total_pages + 1):
            print(f"Обработка страницы {page_number}/{total_pages}")
            payload["pageNumber"] = page_number

            # Выполняем POST-запрос для каждой страницы
            response_text = page.evaluate(
                """
                async ({ apiUrl, requestData }) => {
                    const xhr = new XMLHttpRequest();
                    xhr.open('POST', apiUrl, false);
                    xhr.setRequestHeader('Content-Type', 'application/json');
                    xhr.send(JSON.stringify(requestData));
                    return xhr.responseText;
                }
                """,
                {
                    "apiUrl": api_url,
                    "requestData": payload
                }
            )

            # Обрабатываем ответ
            if "<!DOCTYPE html>" in response_text:
                print(f"Ошибка: HTML на странице {page_number}.")
                continue

            try:
                data = json.loads(response_text)
                links = [f"https://goldapple.ru{product['url']}" for product in data['data']['products']]
                all_links.extend(links)
                print(f"Страница {page_number}: собрано {len(links)} ссылок")
            except Exception as e:
                print(f"Ошибка парсинга страницы {page_number}: {e}")

            # Добавляем задержку между запросами
            time.sleep(0.5)

    except Exception as e:
        print(f"Ошибка: {e}")

    finally:
        # Сохраняем данные в файлa
        if all_links:
            with open('goldapple_links.json', 'w', encoding='utf-8') as f:
                json.dump(all_links, f, ensure_ascii=False, indent=4)
            print(f"Сохранено {len(all_links)} ссылок.")
        else:
            print("Нет данных для сохранения.")

        browser.close()