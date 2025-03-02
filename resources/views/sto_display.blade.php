@extends('layouts.app')

@section('content')
<div class="container">
    <div class="page-header mb-4">
        <div class="header-content">
            <div class="title-section">
                <h2>Data STO</h2>
                <p class="text-muted mb-0">{{ \Carbon\Carbon::parse($currentDate)->setTimezone('Asia/Jakarta')->isoFormat('dddd, D MMMM Y') }}</p>
            </div>
            <form method="GET" action="/sto/display" class="sto-form">
                <div class="form-controls">
                    <div class="date-input">
                        <input type="date" name="date" class="form-control" value="{{ $currentDate }}">
                    </div>
                    <div class="shift-select">
                        <select name="shift" class="form-control" required>
                            <option value="1" {{ $currentShift == '1' ? 'selected' : '' }}>Shift 1</option>
                            <option value="2" {{ $currentShift == '2' ? 'selected' : '' }}>Shift 2</option>
                            <option value="3" {{ $currentShift == '3' ? 'selected' : '' }}>Shift 3</option>
                        </select>
                    </div>
                    <button type="submit" class="btn btn-primary">TAMPILKAN DATA</button>
                    <button type="button" class="btn btn-success" id="exportButton" data-url="{{ route('sto.export', ['date' => $currentDate, 'shift' => $currentShift]) }}">EKSPOR KE EXCEL</button>
                </div>
            </form>
        </div>
    </div>

    <div class="sto-containers">
        <div class="data-display-lhp">
            {{-- Casting 1 --}}
            <div class="casting-box">
            <h4>Casting 1</h4>
            @if(isset($casting1Stos) && $casting1Stos->isNotEmpty())
                @foreach($casting1Stos as $sto)
                    @include('components.sto-table', ['sto' => $sto])
                @endforeach
            @else
                <div class="alert alert-info">
                    Tidak ada data untuk Casting 1 pada tanggal {{ \Carbon\Carbon::parse($currentDate)->setTimezone('Asia/Jakarta')->isoFormat('D MMMM Y') }} Shift {{ $currentShift }}.
                </div>
            @endif
            </div>
            
            {{-- Casting 2 --}}
            <div class="casting-box">
                <h4>Casting 2</h4>
                @if(isset($casting2Stos) && $casting2Stos->isNotEmpty())
                    @foreach($casting2Stos as $sto)
                        @include('components.sto-table', ['sto' => $sto])
                    @endforeach
                @else
                    <div class="alert alert-info">
                        Tidak ada data untuk Casting 2 pada tanggal {{ \Carbon\Carbon::parse($currentDate)->setTimezone('Asia/Jakarta')->isoFormat('D MMMM Y') }} Shift {{ $currentShift }}.
                    </div>
                @endif
            </div>
        </div>

        {{-- Cumulative Box --}}
        <div class="cumulative-box">
            <h4>Kumulatif</h4>
            <div class="table-responsive">
                <table class="table table-bordered">
                    <thead>
                        <tr>
                            <th>Model</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach($models as $model)
                            @php
                                $total = 0;
                                // Penghitungan total untuk casting 1
                                if(isset($casting1Stos) && $casting1Stos->isNotEmpty()) {
                                    foreach($casting1Stos as $sto) {
                                        $total += ($sto->data["machining_r_{$model}"] ?? 0) + ($sto->data["machining_l_{$model}"] ?? 0);
                                        $total += ($sto->data["predrill_r_{$model}"] ?? 0) + ($sto->data["predrill_l_{$model}"] ?? 0);
                                        $total += ($sto->data["repair_r_{$model}"] ?? 0) + ($sto->data["repair_l_{$model}"] ?? 0);
                                        $total += ($sto->data["wagon_r_{$model}"] ?? 0) + ($sto->data["wagon_l_{$model}"] ?? 0);
                                        $total += ($sto->data["otm_r_{$model}"] ?? 0) + ($sto->data["otm_l_{$model}"] ?? 0);
                                        $total += ($sto->data["rib_r_{$model}"] ?? 0) + ($sto->data["rib_l_{$model}"] ?? 0);
                                    }
                                }
                                // Penghitungan total untuk casting 2
                                if(isset($casting2Stos) && $casting2Stos->isNotEmpty()) {
                                    foreach($casting2Stos as $sto) {
                                        $total += ($sto->data["machining_r_{$model}"] ?? 0) + ($sto->data["machining_l_{$model}"] ?? 0);
                                        $total += ($sto->data["predrill_r_{$model}"] ?? 0) + ($sto->data["predrill_l_{$model}"] ?? 0);
                                        $total += ($sto->data["repair_r_{$model}"] ?? 0) + ($sto->data["repair_l_{$model}"] ?? 0);
                                        $total += ($sto->data["wagon_r_{$model}"] ?? 0) + ($sto->data["wagon_l_{$model}"] ?? 0);
                                        $total += ($sto->data["otm_r_{$model}"] ?? 0) + ($sto->data["otm_l_{$model}"] ?? 0);
                                        $total += ($sto->data["rib_r_{$model}"] ?? 0) + ($sto->data["rib_l_{$model}"] ?? 0);
                                    }
                                }
                            @endphp
                            @if($total > 0)
                            <tr>
                                <td>{{ $model }}</td>
                                <td class="text-center">{{ $total }}</td>
                            </tr>
                            @endif
                        @endforeach
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

<style>
/* Page Header */
.page-header {
    background: white;
    padding: 1.5rem;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    margin-bottom: 2rem;
    margin-top: 60px;
}

.header-content {
    align-items: center;
    flex-wrap: wrap;
    gap: 1.5rem;
}

.title-section h2 {
    font-size: 1.75rem;
    font-weight: 600;
    color: #2c3e50;
    margin-bottom: 0.5rem;
}

.form-controls {
    align-items: center;
}

.date-input input,
.shift-select select {
    height: 38px;
    border: 1px solid #e2e8f0;
    border-radius: 6px;
    padding: 0.5rem 1rem;
    font-size: 0.95rem;
    background-color: #f8fafc;
    transition: all 0.2s ease;
    margin-top: 10px;
    margin-bottom: 10px;
}

.date-input input:focus,
.shift-select select:focus {
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    outline: none;
}

.btn-primary {
    margin-top: 10px;
    margin-bottom: 5px;
    background-color: #3b82f6;
    border: none;
    padding: 0.5rem 1.5rem;
    font-weight: 500;
    border-radius: 6px;
    transition: all 0.2s ease;
}

.btn-primary:hover {
    background-color: #2563eb;
    transform: translateY(-1px);
}

.btn-success {
    margin-top: 10px;
    margin-bottom: 10px;
    background-color:rgb(102, 156, 81);
    border: none;
    padding: 0.5rem 1.5rem;
    font-weight: 500;
    border-radius: 6px;
    transition: all 0.2s ease;
}

.btn-succes:hover {
    background-color: #2563eb;
    transform: translateY(-1px);
}
/* Container styles */
.container {
    max-width: 100%;
    padding: 0 1rem;
}

/* Page Header - Keep it full width */
.page-header {
    background: white;
    padding: 1.5rem;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    margin-bottom: 2rem;
}

/* Horizontal scrollable container */
.sto-containers {
    display: flex;
    overflow-x: auto;
    gap: 1rem;
    padding-bottom: 1rem; /* Space for scrollbar */
    -webkit-overflow-scrolling: touch;
    scrollbar-width: thin;
    min-height: 500px; /* Adjust based on your content */
}

/* Customize scrollbar */
.sto-containers::-webkit-scrollbar {
    height: 8px;
}

.sto-containers::-webkit-scrollbar-track {
    background: #f1f5f9;
    border-radius: 4px;
}

.sto-containers::-webkit-scrollbar-thumb {
    background: #cbd5e1;
    border-radius: 4px;
}

/* Main content wrapper */
.data-display-lhp {
    display: flex;
    gap: 1rem;
    flex-shrink: 0;
}

/* Individual boxes */
.casting-box, .cumulative-box {
    background: white;
    padding: 1.5rem;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    min-width: 400px; /* Minimum width for each box */
    flex-shrink: 0;
}

/* Table styles */
.table-responsive {
    width: 100%;
}

.table {
    margin-bottom: 0;
    white-space: nowrap;
}

/* Remove previous media queries that affected the grid layout */
@media (max-width: 768px) {
    .casting-box, .cumulative-box {
        min-width: 300px; /* Slightly smaller on mobile */
    }
    
    .page-header {
        padding: 1rem;
    }
}
</style>

<script>

    document.getElementById('exportButton').addEventListener('click', function() {
        var url = this.getAttribute('data-url');
        window.location = url;
    });
  document.getElementById('modelSearch').addEventListener('keyup', function() {
    var input, filter, table, tr, td, i, txtValue;
    input = document.getElementById('modelSearch');
    filter = input.value.toUpperCase();
    table = document.getElementById('stoTableBody');
    tr = table.getElementsByTagName('tr');

    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName('td')[0];
        if (td) {
            txtValue = td.textContent || td.innerText;
            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = '';
            } else {
                tr[i].style.display = 'none';
            }
        }       
    }
});
</script>
@endsection
