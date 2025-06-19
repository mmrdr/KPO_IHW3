# Online Shop

Проект представляет собой бэкенд для интернет-магазина, реализованный на микросервисной архитектуре. Система включает сервисы для управления заказами и платежами, которые асинхронно взаимодействуют через очередь сообщений RabbitMQ с использованием паттернов Transactional Outbox/Inbox для обеспечения гарантированной доставки.

---

## Быстрый старт

```bash
# Клонировать репозиторий
git clone git@github.com:mmrdr/KPO_IHW3.git

# Перейти в директорию проекта
cd KPO_IHW3

# Собрать и запустить все сервисы
docker-compose up --build
```


---

## Сервисы

### API Gateway (Port 8000)

- **Описание**: Единая точка входа в систему. Маршрутизирует запросы к `Payments Service` и `Orders Service`.
- **Swagger UI**: [http://localhost:8000/](http://localhost:8000/)


### Payments Service (Port 8001)

- **Описание**: Управляет счетами пользователей, операциями пополнения и списания средств. Использует собственную PostgreSQL БД.
- **Swagger UI**: [http://localhost:8001/swagger](http://localhost:8001/swagger)


### Orders Service (Port 8002)

- **Описание**: Отвечает за создание заказов, отслеживание их статусов. Взаимодействует с `Payments Service` через очередь сообщений. Использует собственную PostgreSQL БД.
- **Swagger UI**: [http://localhost:8002/swagger](http://localhost:8002/swagger)

---

## API Эндпоинты

### API Gateway (http://localhost:8000)

- `POST /api/gateway/account` - Создать новый счет для пользователя.
- `POST /api/gateway/account/deposit` - Пополнить баланс счета.
- `GET /api/gateway/account/{userId}` - Получить информацию о счете пользователя.
- `GET /api/gateway/account/{userId}/balance` - Узнать баланс счета.
- `POST /api/gateway/order` - Создать новый заказ (запускает асинхронную оплату).
- `GET /api/gateway/orders/{accountId}` - Получить список всех заказов пользователя.
- `GET /api/gateway/order/{orderId}` - Получить статус конкретного заказа.


### Payments Service (http://localhost:8001/swagger)

- `POST /api/account` - Создать новый счет.
- `POST /api/account/deposit` - Пополнить счет.
- `GET /api/account/{userId}` - Получить информацию о счете.
- `GET /api/account/{id}/balance` - Получить баланс счета.


### Orders Service (http://localhost:8002/swagger)

- `POST /api/orders` - Создать новый заказ.
- `GET /api/orders/all/{accountId}` - Получить все заказы пользователя.
- `GET /api/orders/{orderId}` - Получить статус заказа по ID.

---

## Базы данных

### Payments Database (account-pg)

- **Имя БД**: `account`
- **Схема**: `accounts-storage`
- **Таблицы**:
    - `accounts` - Хранение информации о счетах и балансах пользователей.
    - `inbox_messages` - Входящие сообщения из очереди (Transactional Inbox).
    - `outbox_messages` - Исходящие сообщения для отправки в очередь (Transactional Outbox).


### Orders Database (order-pg)

- **Имя БД**: `order`
- **Схема**: `orders-storage`
- **Таблицы**:
    - `orders` - Хранение информации о заказах и их статусах.
    - `outbox_messages` - Исходящие сообщения для отправки в очередь (Transactional Outbox).

---

## Мониторинг

### Health Checks

- **API Gateway**: [http://localhost:8000/health](http://localhost:8000/health)
- **Payments Service**: [http://localhost:8001/health](http://localhost:8001/health)
- **Orders Service**: [http://localhost:8002/health](http://localhost:8002/health)


### Логи

```bash
# Логи API Gateway
docker-compose logs -f api-gateway

# Логи Payments Service
docker-compose logs -f payment-service

# Логи Orders Service
docker-compose logs -f orders-service
```
