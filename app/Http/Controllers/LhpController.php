<?php

namespace App\Http\Controllers;

use App\Models\Machine;
use App\Models\ProductionData;
use Illuminate\Http\Request;
use Carbon\Carbon;

class LhpController extends Controller
{
    public function create()
    {
        $machines = Machine::orderBy('name')->orderBy('number')->get();
        return view('lhp', compact('machines'));
    }

    public function store(Request $request)
    {
        $request->validate([
            'date' => 'required|date',
            'shift' => 'required|in:1,2,3',
            'data.*.machine_id' => 'required|exists:machines,id',
            'data.*.output_act' => 'nullable|numeric|min:0',
            'data.*.tc' => 'nullable|numeric|min:0',
            'data.*.st' => 'nullable|numeric|min:0',
        ]);

        foreach ($request->data as $item) {
            if (!isset($item['output_act']) && !isset($item['tc']) && !isset($item['st'])) {
                continue; // Skip if no data entered
            }

            // Check if the entry already exists for the given machine, date, and shift
            $existingData = ProductionData::where('machine_id', $item['machine_id'])
                ->where('date', $request->date)
                ->where('shift', $request->shift)
                ->first();

            if ($existingData) {
                // Update existing entry
                $existingData->output_act = $item['output_act'] ?? 0;
                $existingData->tc = $item['tc'] ?? 0;
                $existingData->st = $item['st'] ?? 0;
                $existingData->subtotal = $existingData->output_act + $existingData->tc + $existingData->st;
                $existingData->save();
            } else {
                // Create new entry
                ProductionData::create([
                    'machine_id' => $item['machine_id'],
                    'date' => $request->date,
                    'shift' => $request->shift,
                    'output_act' => $item['output_act'] ?? 0,
                    'tc' => $item['tc'] ?? 0,
                    'st' => $item['st'] ?? 0,
                    'subtotal' => ($item['output_act'] ?? 0) + ($item['tc'] ?? 0) + ($item['st'] ?? 0),
                ]);
            }
        }

        return redirect()->back()->with('success', 'Data berhasil disimpan');
    }

    public function display()
    {
        $machines = Machine::orderBy('name')->orderBy('number')->get();
        // Set locale to Indonesian
        Carbon::setLocale('id');
        
        // If no date provided, use current date but with year 2025
        $date = request('date');
        if (!$date) {
            $currentDate = now()->setTimezone('Asia/Jakarta');
            $date = '2025-' . $currentDate->format('m-d');
        }

        // Get data for all three shifts
        $productionData = [];
        
        for ($shift = 1; $shift <= 3; $shift++) {
            $shiftData = ProductionData::with('machine')
                ->where('date', $date)
                ->where('shift', $shift)
                ->get();

            // Check if shift has any non-zero data
            $hasData = $shiftData->some(function ($data) {
                return $data->output_act > 0 || $data->tc > 0 || $data->st > 0;
            });

            if ($hasData) {
                $productionData[$shift] = $shiftData->keyBy('machine_id');
            } else {
                $productionData[$shift] = collect(); // Empty collection for shifts with no data
            }
        }

        return view('lhp-display', compact('machines', 'productionData', 'date'));
    }
}
