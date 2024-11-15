# Використовуємо офіційний образ Python 3.12
FROM python:3.12-slim

# Встановлюємо оновлення системи та необхідні пакети
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Встановлюємо робочу директорію всередині контейнера
WORKDIR /app

# Копіюємо файл requirements.txt, який містить залежності
COPY requirements.txt .

# Встановлюємо залежності з requirements.txt
RUN pip install --upgrade pip
RUN pip install --no-cache-dir -r requirements.txt

# Копіюємо інші файли до контейнера
COPY . .

# Вказуємо команду для запуску Python скрипту
CMD ["python", "fitmodel_module.py"]