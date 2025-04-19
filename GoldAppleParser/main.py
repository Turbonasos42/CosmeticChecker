import subprocess
import sys
import os


def run_script(script_name):
    try:
        # Проверяем, что файл существует
        if not os.path.exists(script_name):
            print(f"Ошибка: файл {script_name} не найден.")
            return False

        # Запускаем скрипт
        result = subprocess.run([sys.executable, script_name], check=True)
        if result.returncode == 0:
            print(f"Скрипт {script_name} выполнен успешно.")
            return True
        else:
            print(f"Скрипт {script_name} завершился с ошибкой.")
            return False
    except subprocess.CalledProcessError as e:
        print(f"Ошибка при выполнении скрипта {script_name}: {e}")
        return False
    except Exception as e:
        print(f"Неизвестная ошибка: {e}")
        return False


def main():
    # Запускаем links.py для сбора ссылок
    print("Запуск links.py...")
    if not run_script("links.py"):
        print("Прерывание выполнения: не удалось собрать ссылки.")
        return

    # Проверяем, что файл с ссылками создан
    if not os.path.exists("goldapple_links.json"):
        print("Ошибка: файл goldapple_links.json не найден.")
        return

    # Запускаем products.py для сбора данных о товарах
    print("Запуск products.py...")
    if not run_script("products.py"):
        print("Прерывание выполнения: не удалось собрать данные о товарах.")
        return

    print("Все скрипты выполнены успешно.")


if __name__ == "__main__":
    main()