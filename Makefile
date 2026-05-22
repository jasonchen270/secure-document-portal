.PHONY: demo demo-build demo-up demo-down demo-logs demo-open clean demo-local demo-local-stop

# ---- Lightweight local demo (no Docker; SQLite + local filesystem) ----

demo-local:
	@echo "Starting API (SQLite + local-FS) on :5080 and web on :4300..."
	@mkdir -p .demo-pids
	@cd api && ASPNETCORE_ENVIRONMENT=Local nohup dotnet run --no-launch-profile --urls http://localhost:5080 > ../.demo-pids/api.log 2>&1 & echo $$! > ../.demo-pids/api.pid
	@cd web && (test -d node_modules || npm install --no-audit --no-fund) && nohup npx ng serve --host 127.0.0.1 --port 4300 --proxy-config proxy.conf.json > ../.demo-pids/web.log 2>&1 & echo $$! > ../.demo-pids/web.pid
	@echo "Waiting for API..."
	@for i in $$(seq 1 60); do \
	  if curl -fs http://localhost:5080/healthz > /dev/null 2>&1; then echo "API ready."; break; fi; \
	  sleep 2; \
	done
	@echo "Waiting for web (Angular dev server takes ~30s to build)..."
	@for i in $$(seq 1 90); do \
	  if curl -fs http://localhost:4300 > /dev/null 2>&1; then echo "Web ready."; break; fi; \
	  sleep 2; \
	done
	@echo ""
	@echo "==============================================="
	@echo "  Secure Document Portal: LOCAL demo running"
	@echo "  Web:  http://localhost:4300"
	@echo "  API:  http://localhost:5080/openapi/v1.json"
	@echo ""
	@echo "  Logs:  tail -f .demo-pids/api.log  .demo-pids/web.log"
	@echo "  Stop:  make demo-local-stop"
	@echo ""
	@echo "  Seeded accounts (password: ChangeMe!123):"
	@echo "    admin@portal.local      (Admin)"
	@echo "    reviewer@portal.local   (Reviewer)"
	@echo "    uploader@portal.local   (Uploader)"
	@echo "==============================================="
	@open http://localhost:4300 2>/dev/null || true

demo-local-stop:
	@if [ -f .demo-pids/api.pid ]; then kill $$(cat .demo-pids/api.pid) 2>/dev/null || true; rm .demo-pids/api.pid; fi
	@if [ -f .demo-pids/web.pid ]; then kill $$(cat .demo-pids/web.pid) 2>/dev/null || true; rm .demo-pids/web.pid; fi
	@echo "Stopped."

# ---- Full docker-compose demo ----

demo: demo-build demo-up demo-open

demo-build:
	docker compose build

demo-up:
	docker compose up -d
	@echo ""
	@echo "Waiting for API to be ready..."
	@for i in $$(seq 1 60); do \
	  if curl -fs http://localhost:5080/healthz > /dev/null 2>&1; then \
	    echo "API ready."; break; \
	  fi; \
	  sleep 2; \
	done
	@echo "Waiting for web to be ready..."
	@for i in $$(seq 1 30); do \
	  if curl -fs http://localhost:8080 > /dev/null 2>&1; then \
	    echo "Web ready."; break; \
	  fi; \
	  sleep 2; \
	done
	@echo ""
	@echo "==============================================="
	@echo "  Secure Document Portal: demo running"
	@echo "  Web:  http://localhost:8080"
	@echo "  API:  http://localhost:5080/openapi/v1.json"
	@echo ""
	@echo "  Seeded demo accounts (password: ChangeMe!123):"
	@echo "    admin@portal.local      (Admin)"
	@echo "    reviewer@portal.local   (Reviewer)"
	@echo "    uploader@portal.local   (Uploader)"
	@echo "==============================================="

demo-open:
	@open http://localhost:8080 2>/dev/null || xdg-open http://localhost:8080 2>/dev/null || true

demo-down:
	docker compose down

demo-logs:
	docker compose logs -f --tail=100

clean:
	docker compose down -v
