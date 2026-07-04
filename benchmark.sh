#!/usr/bin/env bash
set -uo pipefail

COUNT=${1:-1000}
PROJECT="src/Nimbus.Benchmark/Nimbus.Benchmark.csproj"
TRANSPORTS=(InProcess Redis Nats NatsJetStream Amqp SqlServer Postgres RabbitMq LavinMq)

dotnet build "$PROJECT" -c Release -q

echo
echo "Transport,Count,SendRate_msg_s,Throughput_msg_s,Min_ms,P50_ms,P95_ms,P99_ms,Max_ms"

extract() { echo "$1" | awk -F"$2=" '{print $2}' | awk '{print $1}'; }

for transport in "${TRANSPORTS[@]}"; do
    output=$(timeout 300 dotnet run --project "$PROJECT" -c Release --no-build -- \
        --transport "$transport" --count "$COUNT" 2>&1)
    exit_code=$?

    if [[ $exit_code -ne 0 ]] || ! echo "$output" | grep -q "Results"; then
        echo "$transport,$COUNT,ERROR,ERROR,,,,,"
        continue
    fi

    send_rate=$(echo "$output" | grep "send rate" | awk -F'(' '{print $2}' | awk '{print $1}')
    throughput=$(echo "$output" | grep "Throughput" | awk -F': ' '{print $2}' | awk '{print $1}')
    latency=$(echo "$output" | grep "Latency")
    min=$(extract "$latency" "min")
    p50=$(extract "$latency" "p50")
    p95=$(extract "$latency" "p95")
    p99=$(extract "$latency" "p99")
    max=$(extract "$latency" "max")

    echo "$transport,$COUNT,$send_rate,$throughput,$min,$p50,$p95,$p99,$max"
done
