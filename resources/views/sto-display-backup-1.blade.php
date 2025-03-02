@extends('layouts.app')

@section('content')
<h1>Data STO</h1>
<form method="GET" action="/sto/display" class="form-inline mb-3">
    <div class="form-group mr-2">
        <label for="date" class="mr-2">Tanggal:</label>
        <input type="date" id="date" name="date" class="form-controls" required>
    </div>
    <div class="form-group mr-2">
        <label for="shift" class="mr-2">Shift:</label>
        <select id="shift" name="shift" class="form-controls" required>
            <option value="1">Shift 1</option>
            <option value="2">Shift 2</option>
            <option value="3">Shift 3</option>
        </select>
    </div>
    <div class="form-group mr-2">
        <label for="casting" class="mr-2">Casting:</label>
        <select id="casting" name="casting" class="form-controls" required>
            <option value="Casting 1">Casting 1</option>
            <option value="Casting 2">Casting 2</option>
        </select>
    </div>
    <button type="submit" class="btn btn-primary">Tampilkan</button>
</form>

@if(isset($stos) && $stos->isNotEmpty())
    @foreach($stos as $sto)
        @include('components.sto-table', ['sto' => $sto])
    @endforeach
@endif

<h1>Data STO Terbaru</h1>

@if(isset($latestStos) && $latestStos->isNotEmpty())
    @foreach($latestStos as $sto)
        @include('components.sto-table', ['sto' => $sto])
    @endforeach
@else
    <p>Tidak ada data terbaru.</p>
@endif



@endsection