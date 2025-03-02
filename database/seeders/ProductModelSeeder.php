<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;
use App\Models\ProductModel;

class ProductModelSeeder extends Seeder
{
    public function run()
    {
        $existingModels = [
            'B3F', '54P10CAROUSELL', '54PNONGATTING', 'B65', '2PVINA', '2SX', 'BL5', 
            '1FD10', 'BEJ', '31B', '2DPSIAPCELL', '2DPSUDAHRIB', '2DPBELUMRIB', 
            '1PAINA', '2CC30', '2CC00DISC', '2CC10DRUM', '28D', '28DEMBOSS', 'B5D', 
            'BT3', 'B74', '1LB', '2BU', 'BJM', 'B3MTG', 'B3MSIAPTG', '45PTG', 
            '45PSIAPTG', '1WDTG', '1WDSIAPTG', '484GORDONTG', '3C1', '3C1EMBOSS', 
            '2P2', '2P2EMBOSS', '3KA', '50C', '5D9', '4US', '3S0', '1S7', 
            'UPPERBRACKET', 'UNDERBRACKET', 'UPPER3KA', 'UUB4ST', 'K93', 'K93CAVITY', 
            'K93EMBOSS', 'K93NONGATING', 'XB976', 'XE511', 'A100', 'XA681', 'XCRMD', 
            'GS125PK', 'K1007', 'K9696', 'K9698', 'KMI', 'CSRKAISAR'
        ];

        foreach ($existingModels as $model) {
            ProductModel::create([
                'name' => $model,
                'is_active' => true
            ]);
        }
    }
}