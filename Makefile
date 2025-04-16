.PHONY: build start test down clean
# Makefile for WSPWeatherService

build:
	docker compose build --no-cache

# Start the application and DB
start:
	docker compose up -d wspweatherservice sqlserver

# Run the unit tests
test:
	docker compose run --rm wspweatherservice.tests

# Stop and remove all containers
down:
	docker compose down

# Remove everything (containers, volumes, images)
clean:
	docker compose down --rmi all --volumes --remove-orphans