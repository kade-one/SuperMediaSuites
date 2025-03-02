@extends('layouts.app')

@section('content')
<div class="container">

    <div class="input-data">
        <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <h2>Display Data LHP</h2>
            <p class="text-muted mb-0">{{ \Carbon\Carbon::parse($date)->setTimezone('Asia/Jakarta')->isoFormat('dddd, D MMMM Y') }}</p>
        </div>
        <form method="GET" class="d-flex align-items-center gap-2">
            <div class="d-flex align-items-center">
                <input type="date" name="date" class="form-control form-control-sm" value="{{ $date }}">
                <button type="submit" class="btn btn-sm btn-primary ms-2">CEK LHP</button>
            </div>
        </form>
        </div>
    </div>
    <div class="lhp-containers">
        <div class="shifts-wrapper">
            @foreach([1, 2, 3] as $shiftNumber)
            <div class="shift-box">
            <h4>Shift {{ $shiftNumber }}</h4>
            @php
                $hasData = false;
                foreach ($productionData[$shiftNumber] as $data) {
                    if ($data) {
                        $hasData = true;
                        break;
                    }
                }
            @endphp

            @if($hasData)
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
                                        <td>{{ $productionData[$shiftNumber][$machine->id]->output_act ?? '0' }}</td>
                                        <td>{{ $productionData[$shiftNumber][$machine->id]->tc ?? '0' }}</td>
                                        <td>{{ $productionData[$shiftNumber][$machine->id]->st ?? '0' }}</td>
                                        <td>{{ $productionData[$shiftNumber][$machine->id]->subtotal ?? '0' }}</td>
                                    </tr>
                                    @endif
                                @endforeach
                            @endforeach
                        </tbody>
                    </table>
                </div>
            @else
                <div class="alert alert-info">
                    Data untuk Shift {{ $shiftNumber }} belum tersedia.
                </div>
            @endif
            </div>
            @endforeach
        </div>
        
        <!-- Kolom Kumulatif -->
        <div class="cumulative-box">
            <h4>Kumulatif</h4>
            <div class="table-responsive">
                <table class="table table-bordered">
                    <thead>
                        <tr>
                            <th>Nama Mesin</th>
                            <th>NO MC</th>
                            <th>Total</th>
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
                            $grandTotal = 0;
                        @endphp

                        @foreach($machineGroups as $machineName => $numbers)
                            @foreach($numbers as $number)
                                @php
                                    $machine = $machines->where('name', $machineName)
                                                      ->where('number', $number)
                                                      ->first();
                                    $total = 0;
                                    if ($machine) {
                                        foreach([1, 2, 3] as $shift) {
                                            if (isset($productionData[$shift][$machine->id])) {
                                                $total += $productionData[$shift][$machine->id]->subtotal ?? 0;
                                            }
                                        }
                                        $grandTotal += $total;
                                    }
                                @endphp
                                @if($machine)
                                <tr>
                                    <td>{{ $machineName }}</td>
                                    <td>{{ $number }}</td>
                                    <td class="text-center">{{ $total }}</td>
                                </tr>
                                @endif
                            @endforeach
                        @endforeach
                        <tr>
                            <td colspan="2" class="text-center font-weight-bold border-top">Total</td>
                            <td class="text-center font-weight-bold border-top">{{ $grandTotal }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
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
        overflow-x: auto;
        scroll-behavior: smooth;
        -webkit-overflow-scrolling: touch;
    }

    .shifts-wrapper {
        display: flex;
        gap: 15px;
        flex: 3;
        min-width: min-content;
    }

    .shift-box, .cumulative-box {
        flex: 1;
        min-width: 380px; /* Increased from 300px */
        background: white;
        padding: 1rem;
        border-radius: 6px;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .cumulative-box {
        flex: 1;
        background: #fff;
        min-width: 350px; /* Slightly smaller than shift boxes since fewer columns */
    }

    /* Adjust table cell widths */
    .table th, .table td {
        white-space: nowrap; /* Prevent text wrapping */
        min-width: 70px; /* Minimum width for number columns */
    }

    .table th:first-child, 
    .table td:first-child {
        min-width: 120px; /* Wider for machine names */
    }

    /* Make sure the tables don't shrink too much */
    .table {
        width: 100%;
        min-width: 350px; /* Minimum table width */
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
            flex-direction: row;
            padding-bottom: 1rem;
        }
        
        .shifts-wrapper {
            flex-direction: row;
            flex-wrap: nowrap;
        }

        .shift-box, .cumulative-box {
            margin-bottom: 0;
        }
        
        .table-responsive {
            position: relative;
        }
    }

    .lhp-containers::-webkit-scrollbar {
        height: 8px;
    }

    .lhp-containers::-webkit-scrollbar-track {
        background: #f1f1f1;
    }

    .lhp-containers::-webkit-scrollbar-thumb {
        background: #888;
        border-radius: 4px;
    }

    .lhp-containers::-webkit-scrollbar-thumb:hover {
        background: #555;
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
@endsection