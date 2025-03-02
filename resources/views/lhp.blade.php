@extends('layouts.app')

@section('content')
    <div class="container">
        <div class="input-data">
            <h2>Input Data LHP</h2>
                @if(session('success'))
            <div class="alert alert-success">
                {{ session('success') }}
            </div>
        </div>
    @endif

    <form action="{{ route('lhp.store') }}" method="POST">
        @csrf
            <div class="row mb-3">
                <div class="col-md-6">
                    <label for="date">Tanggal:</label>
                    <input type="date" name="date" class="form-control" required value="{{ date('Y-m-d') }}">
                </div>
                <div class="col-md-6">
                    <label for="shift">Shift:</label>
                    <select name="shift" class="form-control" required>
                    <option value="1">Shift 1</option>
                    <option value="2">Shift 2</option>
                    <option value="3">Shift 3</option>
                    </select>
                </div>
            </div>
    </div>
        

        <div class="table-responsive">
                <table class="table table-bordered">
                    <thead>
                        <tr>
                            <th>Nama Mesin</th>
                            <th>NO MC</th>
                            <th>OUTPUT Act</th>
                            <th>TC</th>
                            <th>ST</th>
                            <th>Sub</th>
                        </tr>
                    </thead>
                    <tbody>
                        @php
                            $machineGroups = [
                                'Carousell 1' => [1, 2, 3, 4, 5, 6, 7, 8],
                                'Carousell 2' => [1, 2, 3, 4, 5, 6, 7, 8],
                                'Gantry 1' => [1, 2, 3, 4],
                                'Gantry 2' => [1, 2, 3, 4],
                                'Alfa 5' => [14, 15, 16, 17],
                                'Alfa 4' => [18, 19, 20, 21],
                                'Alfa 3' => [22, 23, 24],
                                'Alfa 2' => [25, 26, 27],
                                'Alfa 1' => [28, 29, 30],
                                'Grav P3' => range(33, 42),
                                'Grav P4' => range(43, 56)
                            ];
                        @endphp

                        @foreach($machineGroups as $machineName => $numbers)
                            @foreach($numbers as $number)
                                @php
                                    $machine = $machines->where('name', $machineName)
                                                    ->where('number', $number)
                                                    ->first();
                                @endphp
                                @if($machine)
                                <tr>
                                    <td>{{ $machineName }}</td>
                                    <td>{{ $number }}</td>
                                    <td>
                                        <input type="hidden" name="data[{{ $machine->id }}][machine_id]" value="{{ $machine->id }}">
                                        <input type="number" name="data[{{ $machine->id }}][output_act]" class="form-control output-act" placeholder="">
                                    </td>
                                    <td>
                                        <input type="number" name="data[{{ $machine->id }}][tc]" class="form-control tc" placeholder="">
                                    </td>
                                    <td>
                                        <input type="number" name="data[{{ $machine->id }}][st]" class="form-control st" placeholder="">
                                    </td>
                                    <td>
                                        <input type="number" class="form-control subtotal" readonly>
                                    </td>
                                </tr>
                                @endif
                            @endforeach
                        @endforeach
                    </tbody>
                </table>
            </div>

            <button type="submit" class="btn btn-primary">Simpan Data</button>
        </form>
    </div>
<style>
        
    .form-control {
        padding: 15px;
        font-size: large;
    }
    .btn {
        background-color: #666;
        padding-top: 15px;
        padding-bottom: 15px;
        padding-left: 30px;
        padding-right: 30px;
        margin: 15px;
    }
    .lhp-containers {
        display: flex;
        gap: 15px;
        margin: 0 auto;
        padding: 1rem;
        background: #f8f9fa;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .shifts-wrapper {
        display: flex;
        gap: 15px;
        flex: 3;
    }

    .shift-box, .cumulative-box {
        flex: 1;
        min-width: 0;
        background: white;
        padding: 1rem;
        border-radius: 6px;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .cumulative-box {
        flex: 1;
        background: #fff;
    }

    .cumulative-box h4 {
        text-align: center;
        margin-bottom: 1rem;
        color: #333;
        font-size: 1.1rem;
        padding-bottom: 0.5rem;
        border-bottom: 2px solid #28a745;
    }

    @media (max-width: 1200px) {
        .lhp-containers {
            flex-direction: column;
        }
        
        .shifts-wrapper {
            flex-direction: column;
        }

        .shift-box, .cumulative-box {
            margin-bottom: 1rem;
        }
    }

    .input-data {
    padding: 1.5rem;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    margin-bottom: 2rem;
    margin-top: 60px;
    background-color: white;
    }
</style>
<script>
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('tr').forEach(row => {
        const inputs = row.querySelectorAll('input[type="number"]:not([readonly])');
        const subtotal = row.querySelector('.subtotal');

        inputs.forEach(input => {
            input.addEventListener('input', () => {
                const outputAct = parseInt(row.querySelector('.output-act').value) || 0;
                const tc = parseInt(row.querySelector('.tc').value) || 0;
                const st = parseInt(row.querySelector('.st').value) || 0;
                const total = outputAct + tc + st;
                
                if (total > 0) {
                    subtotal.value = total;
                } else {
                    subtotal.value = '';
                }
            });
        });
    });
});
document.getElementById('dataForm').addEventListener('submit', function(event) {
    event.preventDefault();
    alert('Data LHP berhasil disimpan');
    this.submit();
});
</script>
@endsection
