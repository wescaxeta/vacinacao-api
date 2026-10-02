#!/bin/sh
# Teste de fumaça contra a API no ar, usando os dados de demonstração.
# Uso: sh tests/smoke.sh [url-base]   (padrão: http://localhost:8090)
set -eu

BASE="${1:-http://localhost:8090}"
FALHAS=0

verificar() {
    descricao="$1"; esperado="$2"; obtido="$3"
    if [ "$esperado" = "$obtido" ]; then
        echo "  ok     $descricao ($obtido)"
    else
        echo "  FALHOU $descricao: esperado $esperado, obtido $obtido"
        FALHAS=$((FALHAS + 1))
    fi
}

status() { curl -s -o /dev/null -w '%{http_code}' "$@"; }
apta() { curl -s "$BASE/propriedades/$1/aptidao?especie=bovino" | grep -o '"apta":[a-z]*' | cut -d: -f2; }

echo "Smoke test em $BASE"

verificar "health check" 200 "$(status "$BASE/health")"
verificar "documentação OpenAPI" 200 "$(status "$BASE/openapi/v1.json")"
verificar "GO000001 vacinou tudo: apta" true "$(apta GO000001)"
verificar "GO000003 sem brucelose: bloqueada" false "$(apta GO000003)"
verificar "MT000010 com raiva no prazo: apta" true "$(apta MT000010)"
verificar "espécie inválida" 400 "$(status "$BASE/propriedades/GO000001/aptidao?especie=gato")"

if [ "$FALHAS" -gt 0 ]; then
    echo "$FALHAS verificação(ões) falharam."
    exit 1
fi

echo "Tudo certo."
