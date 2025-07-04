# Cosmetic Checker

Веб-сайт для анализа состава косметики с автоматизированным сбором данных.

## Команда
- Анна Косарева — frontend-разработчик, аналитик
- Бунтова Дарья — frontend-разработчик, аналитик
- Глеб Касимов — backend-разработчик

## Цель проекта
Разработка платформы для анализа состава косметических продуктов с функциями:
- Поиск по составу, бренду, названию, типу кожи
- Оценка безопасности состава
- Рекомендации на основе предпочтений
- Отзывы и рейтинги
- Избранные товары и подборки
- Сравнение продуктов

## Технологии
- **Backend**: C#, Entity Framework Core
- **База данных**: PostgreSQL (Npgsql)
- **Парсинг**: Playwright

## Структура базы данных

### Таблица Products
- id, name, brand, description, category, price
- ingredients, safety_rating, analogues, source

### Таблица Ingredients_info
- id, name, description, safety_level, source

### Таблица User
- id, first_name, last_name, email, password
- role_id, skin_type, allergies, preferred_ingredients

### Таблица Reviews
- id, user_id, product_id, rating, text, is_approved, creation_date

### Таблица Favorites
- id, user_id, product_id

## Алгоритм работы
1. Сбор данных с goldapple.ru через Playwright
2. Сохранение данных в JSON
3. Загрузка в PostgreSQL с помощью C#
4. Управление БД через Entity Framework Core
