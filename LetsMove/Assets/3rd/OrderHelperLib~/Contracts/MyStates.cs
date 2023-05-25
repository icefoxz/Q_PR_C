using System;
using System.Collections.Generic;
using System.Text;

namespace OrderHelperLib.Contracts
{
    public enum MyStates : byte
    {
        Johor = 1,
        Kedah = 2,
        Kelantan = 3,
        Melaka = 4,
        NegeriSembilan = 5,
        Pahang = 6,
        PulauPinang = 7,
        Perak = 8,
        Perlis = 9,
        Selangor = 10,
        Terengganu = 11,
        Sabah = 12,
        Sarawak = 13,
        //KualaLumpur = 14,
        //Labuan = 15,
        //Putrajaya = 16
    }

    public static class MyStatesExtension
    {
        public static string Text(this MyStates state) => state switch
        {
            MyStates.Johor => "Johor",
            MyStates.Kedah => "Kedah",
            MyStates.Kelantan => "Kelantan",
            MyStates.Melaka => "Melaka",
            MyStates.NegeriSembilan => "Negeri Sembilan",
            MyStates.Pahang => "Pahang",
            MyStates.PulauPinang => "Pulau Pinang",
            MyStates.Perak => "Perak",
            MyStates.Perlis => "Perlis",
            MyStates.Selangor => "Selangor",
            MyStates.Terengganu => "Terengganu",
            MyStates.Sabah => "Sabah",
            MyStates.Sarawak => "Sarawak",
            //MyStates.KualaLumpur => "Kuala Lumpur",
            //MyStates.Labuan => "Labuan",
            //MyStates.Putrajaya => "Putrajaya",
            _ => throw new NotImplementedException(),
        };
    }
}
