using TMMLibrary.TMM;

var baseDir = Environment.GetEnvironmentVariable("EXTRACTED_DATA_DIR") ?? ".";
baseDir += "/intermediate/modelcache";
        
// TmmFile.Decode($"{baseDir}/spc/units/reginleif/reginleif_spear.tmm");
var tmm = TmmFile.Decode($"{baseDir}/spc/units/reginleif/reginleif_spc.tmm");
// var tmm = TmmFile.Decode($"{baseDir}/atlantean/units/cavalry/contarius/contarius_hero.tmm");
foreach (var model in tmm.ModelInfos)
{
    Console.WriteLine($"attach point count = {model.AttachPointCount}");
    Console.WriteLine($"unknown count = {model.UnknownCount}");
    Console.WriteLine($"bone count = {model.BoneCount}");
    Console.WriteLine($"unknown3 = {model.Unknown3:X}");
}
// TmaFile.Decode($"{baseDir}/spc/units/reginleif/anim/reginleif_idle_a.tma");
// TmaFile.Decode($"{baseDir}/spc/units/reginleif/anim/igc/fott_regi_26_thats_right.tma");
// TmaFile.Decode($"{baseDir}/atlantean/units/cavalry/contarius/anim/contarius_horse_birth_a.tma");
