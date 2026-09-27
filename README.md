TestProd
REST API для обработки HTML-страницы и связанных с ней данных.

Возможности

API принимает JSON с:
CSS-селектором и именем HTML-атрибута;
URL в Base64;
HTML-страницей в Base64;
зашифрованным текстом и AES-256 ключом в Base64.

В процессе обработки:
- выполняется валидация входных данных с FluentValidation;
- декодируются URL и HTML;
- HTML разбирается с помощью AngleSharp;
- выполняется поиск элементов по CSS-селектору;
- извлекаются значения указанного атрибута;
- найденные элементы сохраняются в PostgreSQL через Dapper;
- из HTML извлекаются email-адреса;
- выполняется AES-256 ECB расшифровка текста;
- результат возвращается в JSON.

Технологии
.NET 10
ASP.NET Core Web API
PostgreSQL 18
Dapper
FluentValidation
AngleSharp
Docker Compose
Swagger
System.Text.Json

Запуск
Требуется Docker Desktop.
Из корня проекта выполнить:
docker compose up --build

После запуска:
API: http://localhost:8090
Swagger: http://localhost:8090/api/swagger
pgAdmin: http://localhost:8080
