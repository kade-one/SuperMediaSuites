<?php

namespace App\Http\Controllers;

use Illuminate\Support\Facades\Log;
use Illuminate\Http\Request;
use App\Models\Sto;
use Carbon\Carbon;
use App\Models\ProductModel;
use Illuminate\Support\Facades\View;
use App\Exports\StoExport; // Buat file export yang sesuai
use Maatwebsite\Excel\Facades\Excel;
use App\Exports\Casting1Export;
use App\Exports\Casting2Export;
use App\Exports\CumulativeExport;
class StoController extends Controller
{
    // Metode store tetap sama
    public function store(Request $request)
    {
        $request->validate([
            'date' => 'required|date',
            'operator' => 'required|string|max:255',
            'casting' => 'required|string|max:50',
            'shift' => 'required|string|max:50',
            'jam_sto' => 'required|string|max:50',
            'fm' => 'required|string|max:50',
        ]);

        $sto = new Sto();
        $sto->date = $request->input('date');
        $sto->operator = $request->input('operator');
        $sto->casting = $request->input('casting');
        $sto->shift = $request->input('shift');
        $sto->jam_sto = $request->input('jam_sto');
        $sto->fm = $request->input('fm');

        // Hapus token _token dan model dari data
        $data = $request->except(['date', 'operator', 'casting', 'shift', 'jam_sto', 'fm', '_token']);
        
        // Hilangkan spasi dan titik pada nama model
        $formattedData = [];
        foreach ($data as $key => $value) {
            $formattedKey = str_replace([' ', '.'], '', $key);
            $formattedData[$formattedKey] = $value;
        }

        $sto->data = json_encode($formattedData);
        $sto->save();

        return redirect()->back()->with('success', 'Data STO berhasil disimpan');
    }

    // Metode show yang diperbarui: menampilkan kedua casting
    public function show(Request $request)
    {
        // Set locale to Indonesian
        Carbon::setLocale('id');
        
        // Use WIB timezone and set year to 2025
        $date = $request->input('date');
        if (!$date) {
            $currentDate = now()->setTimezone('Asia/Jakarta');
            $date = '2025-' . $currentDate->format('m-d');
        }
        $shift = $request->input('shift') ?? $this->getCurrentShift();
        
        $formatData = function($stos) {
            foreach ($stos as $sto) {
                $data = json_decode($sto->data, true);
                $formattedData = [];
                foreach ($data as $key => $value) {
                    $formattedKey = str_replace([' ', '.'], '', $key);
                    $formattedData[$formattedKey] = $value;
                }
                $sto->data = $formattedData;
            }
            return $stos;
        };
        
        $casting1Stos = Sto::where('date', $date)
                            ->where('shift', $shift)
                            ->where('casting', 'Casting 1')
                            ->latest('created_at')
                            ->take(1)
                            ->get();
        
        // Jika tidak ada data untuk hari ini, coba kemarin
        if ($casting1Stos->isEmpty()) {
            $yesterday = Carbon::parse($date)->subDay()->format('Y-m-d');
            $casting1Stos = Sto::where('date', $yesterday)
                                ->where('shift', $shift)
                                ->where('casting', 'Casting 1')
                                ->latest('created_at')
                                ->take(1)
                                ->get();
        }
        
        // Ambil data untuk tabel di Casting 2
        $casting2Stos = Sto::where('date', $date)
                            ->where('shift', $shift)
                            ->where('casting', 'Casting 2')
                            ->latest('created_at')
                            ->take(1)
                            ->get();
        
        // Jika tidak ada data untuk hari ini, coba kemaren
        if ($casting2Stos->isEmpty()) {
            $yesterday = Carbon::parse($date)->subDay()->format('Y-m-d');
            $casting2Stos = Sto::where('date', $yesterday)
                                ->where('shift', $shift)
                                ->where('casting', 'Casting 2')
                                ->latest('created_at')
                                ->take(1)
                                ->get();
        }
        
        // Format data
        $casting1Stos = $formatData($casting1Stos);
        $casting2Stos = $formatData($casting2Stos);
        
        return view('sto_display', [
            'casting1Stos' => $casting1Stos,
            'casting2Stos' => $casting2Stos,
            'currentDate' => $date,
            'currentShift' => $shift
        ]);
    }

    // Method untuk halaman depan - menampilkan data terbaru tanpa filter
    public function index()
    {
        // Format data helper function
        $formatData = function($stos) {
            foreach ($stos as $sto) {
                $data = json_decode($sto->data, true);
                $formattedData = [];
                foreach ($data as $key => $value) {
                    $formattedKey = str_replace([' ', '.'], '', $key);
                    $formattedData[$formattedKey] = $value;
                }
                $sto->data = $formattedData;
            }
            return $stos;
        };
        
        // Ambil data terbaru untuk Casting 1
        $casting1Stos = Sto::where('casting', 'Casting 1')
                          ->latest('date')
                          ->latest('created_at')
                          ->take(1)
                          ->get();
        
        // Ambil data terbaru untuk Casting 2
        $casting2Stos = Sto::where('casting', 'Casting 2')
                          ->latest('date')
                          ->latest('created_at')
                          ->take(1)
                          ->get();
        
        // Format data
        $casting1Stos = $formatData($casting1Stos);
        $casting2Stos = $formatData($casting2Stos);
        
        // Set locale to Indonesian
        Carbon::setLocale('id');
        
        // Use WIB timezone and set year to 2025
        $currentDate = now()->setTimezone('Asia/Jakarta');
        $currentDate = '2025-' . $currentDate->format('m-d');
        $currentShift = $this->getCurrentShift();
        
        if ($casting1Stos->isNotEmpty()) {
            $currentDate = $casting1Stos->first()->date;
            $currentShift = $casting1Stos->first()->shift;
        } elseif ($casting2Stos->isNotEmpty()) {
            $currentDate = $casting2Stos->first()->date;
            $currentShift = $casting2Stos->first()->shift;
        }
        
        return view('sto_display', [
            'casting1Stos' => $casting1Stos,
            'casting2Stos' => $casting2Stos,
            'currentDate' => $currentDate,
            'currentShift' => $currentShift
        ]);
    }
    
    // Helper function untuk mendapatkan shift saat ini berdasarkan waktu
    private function getCurrentShift()
    {
        $hour = now()->setTimezone('Asia/Jakarta')->hour;
        
        if ($hour >= 6 && $hour < 14) {
            return "2"; // Shift 1: 06:00 - 14:00
        } elseif ($hour >= 14 && $hour < 22) {
            return "3"; // Shift 2: 14:00 - 22:00
        } else {
            return "1"; // Shift 3: 22:00 - 06:00
        }
    }

    public function __construct()
{
    // Inject models ke semua views
    View::share('models', ProductModel::where('is_active', true)
                                    ->orderBy('name')
                                    ->pluck('name'));
}

public function export(Request $request)
{
    $date = $request->get('date', now()->format('Y-m-d'));
    $shift = $request->get('shift', $this->getCurrentShift());
    
    $models = ProductModel::where('is_active', true)
                         ->orderBy('name')
                         ->pluck('name');
    
    return Excel::download(new StoExport($date, $shift, $models), 
        "sto_data_{$date}_shift_{$shift}.xlsx");
}

}