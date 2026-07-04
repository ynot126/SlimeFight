public class EnemyData
{
    readonly string id;
    public EntityStat stat;
    public EnemyData(string aId)
    {
        id = aId;
    }

    public EnemyData SetStat(EntityStat aStat)
    {
        stat = aStat;
        return this;
    }
}