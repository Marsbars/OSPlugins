using System.Collections.Generic;
using wManager.Wow.Enums;


public static class Professions
{
    public static List<SkillLine> Primary = new List<SkillLine>
        {
            SkillLine.Alchemy,
            SkillLine.Blacksmithing,
            SkillLine.Enchanting,
            SkillLine.Engineering,
            SkillLine.Herbalism,
            SkillLine.Inscription,
            SkillLine.Jewelcrafting,
            SkillLine.Leatherworking,
            SkillLine.Mining,
            SkillLine.Skinning,
            SkillLine.Tailoring,
        };
    public static List<SkillLine> Secondary = new List<SkillLine>
        {
            SkillLine.Archaeology,
            SkillLine.Cooking,
            SkillLine.FirstAid,
            SkillLine.Fishing,
        };
}

