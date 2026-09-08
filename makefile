.PHONY: up migrations start

up:
	docker compose up -d

migrations:
	docker compose run --rm migrations

start:
	docker compose run --rm --service-ports api