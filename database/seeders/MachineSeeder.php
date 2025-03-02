<?php
// database/seeders/MachineSeeder.php

namespace Database\Seeders;

use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\DB;

class MachineSeeder extends Seeder
{
    public function run()
    {
        $machines = [
            ['name' => 'Carousell 1', 'numbers' => range(1, 8)],
            ['name' => 'Carousell 2', 'numbers' => range(1, 8)],
            ['name' => 'Gantry 1', 'numbers' => range(1, 4)],
            ['name' => 'Gantry 2', 'numbers' => range(1, 4)],
            ['name' => 'Alfa 5', 'numbers' => range(14, 17)],
            ['name' => 'Alfa 4', 'numbers' => range(18, 21)],
            ['name' => 'Alfa 3', 'numbers' => range(22, 24)],
            ['name' => 'Alfa 2', 'numbers' => range(25, 27)],
            ['name' => 'Alfa 1', 'numbers' => range(28, 30)],
            ['name' => 'Grav P3', 'numbers' => range(33, 42)],
            ['name' => 'Grav P4', 'numbers' => range(43, 56)]
        ];

        foreach ($machines as $machine) {
            foreach ($machine['numbers'] as $number) {
                DB::table('machines')->insert([
                    'name' => $machine['name'],
                    'number' => $number,
                    'created_at' => now(),
                    'updated_at' => now()
                ]);
            }
        }
    }
}