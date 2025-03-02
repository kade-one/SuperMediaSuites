<?php

namespace App\Exports;

use App\Models\Sto;
use Maatwebsite\Excel\Concerns\FromCollection;
use Maatwebsite\Excel\Concerns\WithEvents;
use Maatwebsite\Excel\Concerns\WithTitle;
use Maatwebsite\Excel\Concerns\ShouldAutoSize;
use Maatwebsite\Excel\Events\AfterSheet;
use PhpOffice\PhpSpreadsheet\Style\Alignment;
use PhpOffice\PhpSpreadsheet\Style\Border;
use PhpOffice\PhpSpreadsheet\Style\Fill;

class StoExport implements FromCollection, WithEvents, WithTitle, ShouldAutoSize
{
    protected $date;
    protected $shift;
    protected $models;

    public function __construct($date, $shift, $models)
    {
        $this->date = $date;
        $this->shift = $shift;
        $this->models = $models;
    }

    public function collection()
    {
        // Get data for Casting 1 and 2
        $casting1Data = Sto::where('date', $this->date)
            ->where('shift', $this->shift)
            ->where('casting', 'Casting 1')
            ->first();

        $casting2Data = Sto::where('date', $this->date)
            ->where('shift', $this->shift)
            ->where('casting', 'Casting 2')
            ->first();

        $c1Data = $casting1Data ? json_decode($casting1Data->data, true) : [];
        $c2Data = $casting2Data ? json_decode($casting2Data->data, true) : [];

        // Initialize empty array for data
        $data = [];

        // First row - Main headers
        $firstRow = [
            '', // Empty cell for A1
            'CASTING 1', '', '', '', '', '', '', '', '', '', '', '', 
            '', // Spacer column N
            'CASTING 2', '', '', '', '', '', '', '', '', '', '', '','',
            'Total'
        ];
        $data[] = $firstRow;

        // Second row - Process headers
        $processHeaders = [
            'Model',
            'Siap Machining', '',
            'Siap Predrill', '',
            'Siap Repair', '',
            'Wagon Gravity', '',
            'Line OTM', '',
            'Siap Rib', ''
        ];

        $secondRow = [];
        foreach ($processHeaders as $header) {
            $secondRow[] = $header;
        }
        $secondRow[] = ''; // Spacer column N
        foreach ($processHeaders as $header) {
            $secondRow[] = $header;
        }
        $secondRow[] = ''; // Total
        $data[] = $secondRow;

        // Third row - R/L headers
        $thirdRow = [''];
        // R/L for Casting 1
        for ($i = 0; $i < 6; $i++) {
            $thirdRow[] = 'R';
            $thirdRow[] = 'L';
        }
        $thirdRow[] = ''; // Spacer column N
        $thirdRow[] = ''; // Model column for Casting 2
        // R/L for Casting 2
        for ($i = 0; $i < 6; $i++) {
            $thirdRow[] = 'R';
            $thirdRow[] = 'L';
        }
        $thirdRow[] = ''; // Total
        $data[] = $thirdRow;

        // Data rows
        foreach ($this->models as $model) {
            $modelKey = str_replace([' ', '.'], '', $model);
            $row = [];
            $totalForModel = 0;

            // Casting 1 data
            $row[] = $model;
            $processes = ['machining', 'predrill', 'repair', 'wagon', 'otm', 'rib'];
            foreach ($processes as $process) {
                $rValue = $c1Data["{$process}_r_$modelKey"] ?? 0;
                $lValue = $c1Data["{$process}_l_$modelKey"] ?? 0;
                $row[] = $rValue;
                $row[] = $lValue;
                $totalForModel += $rValue + $lValue;
            }

            $row[] = ''; // Spacer column N

            // Casting 2 data
            $row[] = $model;
            foreach ($processes as $process) {
                $rValue = $c2Data["{$process}_r_$modelKey"] ?? 0;
                $lValue = $c2Data["{$process}_l_$modelKey"] ?? 0;
                $row[] = $rValue;
                $row[] = $lValue;
                $totalForModel += $rValue + $lValue;
            }

            $row[] = $totalForModel;
            $data[] = $row;
        }

        return collect($data);
    }

    public function registerEvents(): array
    {
        return [
            AfterSheet::class => function(AfterSheet $event) {
                $sheet = $event->sheet;
                $lastRow = count($this->models) + 3;

                // Style untuk semua header
                $headerStyle = [
                    'font' => [
                        'bold' => true,
                    ],
                    'alignment' => [
                        'horizontal' => Alignment::HORIZONTAL_CENTER,
                    ],
                    'fill' => [
                        'fillType' => Fill::FILL_SOLID,
                        'startColor' => ['rgb' => 'E2EFDA'],
                    ],
                ];

                // Apply header style
                $sheet->getStyle('A1:AB' . $lastRow)->applyFromArray($headerStyle);

                // Merge cells untuk header utama
                $sheet->mergeCells('B1:M1');  // Casting 1
                $sheet->mergeCells('O1:AA1');  // Casting 2

                // Merge cells untuk Casting 1
                $sheet->mergeCells('B2:C2');  // Siap Machining
                $sheet->mergeCells('D2:E2');  // Siap Predrill
                $sheet->mergeCells('F2:G2');  // Siap Repair
                $sheet->mergeCells('H2:I2');  // Wagon Gravity
                $sheet->mergeCells('J2:K2');  // Line OTM
                $sheet->mergeCells('L2:M2');  // Siap Rib

                // Merge cells untuk Casting 2
                $sheet->mergeCells('P2:Q2');  // Siap Machining
                $sheet->mergeCells('R2:S2');  // Siap Predrill
                $sheet->mergeCells('T2:U2');  // Siap Repair
                $sheet->mergeCells('V2:W2');  // Wagon Gravity
                $sheet->mergeCells('X2:Y2');  // Line OTM
                $sheet->mergeCells('Z2:AA2');  // Siap Rib

                // Border style
                $borderStyle = [
                    'borders' => [
                        'allBorders' => [
                            'borderStyle' => Border::BORDER_THIN,
                        ],
                    ],
                ];

                // Apply borders
                $sheet->getStyle('A1:M' . $lastRow)->applyFromArray($borderStyle);  // Casting 1
                $sheet->getStyle('O1:AB' . $lastRow)->applyFromArray($borderStyle);  // Casting 2
                $sheet->getStyle('AB1:AB' . $lastRow)->applyFromArray($borderStyle);  // Total

                // Set column width untuk spacer
                $sheet->getColumnDimension('N')->setWidth(2);

                // Center alignment untuk semua cell
                $sheet->getStyle('A1:AB' . $lastRow)->getAlignment()->setHorizontal(Alignment::HORIZONTAL_CENTER);
                
                // Left alignment untuk kolom Model
                $sheet->getStyle('A2:A' . $lastRow)->getAlignment()->setHorizontal(Alignment::HORIZONTAL_LEFT);
                $sheet->getStyle('O2:O' . $lastRow)->getAlignment()->setHorizontal(Alignment::HORIZONTAL_LEFT);

                // Bold untuk kolom Total
                $sheet->getStyle('AB1:AB' . $lastRow)->getFont()->setBold(true);
            },
        ];
    }

    public function title(): string
    {
        return "STO Data - " . \Carbon\Carbon::parse($this->date)->format('d/m/Y') . " Shift " . $this->shift;
    }
}