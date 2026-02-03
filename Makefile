SOLUTION = PreciousMetalsManager/PreciousMetalsManager.slnx
PROJECT = PreciousMetalsManager/PreciousMetalsManager.csproj
TEST_PROJECT = tests/PreciousMetalsManager.Tests.csproj

.PHONY: all build clean test run format restore

all: build

restore:
	dotnet restore $(SOLUTION)

build: restore
	dotnet build $(SOLUTION) --configuration Release --no-restore

clean:
	dotnet clean $(SOLUTION)

run:
	dotnet run --project $(PROJECT) --configuration Release

test:
	dotnet test $(TEST_PROJECT) --no-build --configuration Release

format:
	dotnet format $(SOLUTION)

# Usage:
#   make build   - Build the solution in Release mode
#   make run     - Run the WPF application
#   make test    - Run all unit tests
#   make clean   - Clean build artifacts
#   make format  - Format code using dotnet-format
#   make restore - Restore NuGet packages
