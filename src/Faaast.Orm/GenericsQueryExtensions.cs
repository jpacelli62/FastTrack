using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Faaast.Orm.Reader;

namespace Faaast.Orm
{
    public class FaaastTuple<TA, TB>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public FaaastTuple(TA A, TB B)
        {
            this.RecordA = A;
            this.RecordB = B;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB>> Fetch<TA, TB>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB)))
            {
                yield return new FaaastTuple<TA, TB>((TA)row[0], (TB)row[1]);
            }
        }

        public static FaaastTuple<TA, TB> FirstOrDefault<TA, TB>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB> Single<TA, TB>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB> result = default;
            foreach (var item in command.Fetch<TA, TB>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB>> FetchAsync<TA, TB>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB)))
            {
                yield return new FaaastTuple<TA, TB>((TA)row[0], (TB)row[1]);
            }
        }

        public static async Task<FaaastTuple<TA, TB>> FirstOrDefaultAsync<TA, TB>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB>> SingleAsync<TA, TB>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB> result = default;
            await foreach (var item in command.FetchAsync<TA, TB>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public FaaastTuple(TA A, TB B, TC C)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC>> Fetch<TA, TB, TC>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC)))
            {
                yield return new FaaastTuple<TA, TB, TC>((TA)row[0], (TB)row[1], (TC)row[2]);
            }
        }

        public static FaaastTuple<TA, TB, TC> FirstOrDefault<TA, TB, TC>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC> Single<TA, TB, TC>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC> result = default;
            foreach (var item in command.Fetch<TA, TB, TC>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC>> FetchAsync<TA, TB, TC>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC)))
            {
                yield return new FaaastTuple<TA, TB, TC>((TA)row[0], (TB)row[1], (TC)row[2]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC>> FirstOrDefaultAsync<TA, TB, TC>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC>> SingleAsync<TA, TB, TC>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD>> Fetch<TA, TB, TC, TD>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD> FirstOrDefault<TA, TB, TC, TD>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD> Single<TA, TB, TC, TD>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD>> FetchAsync<TA, TB, TC, TD>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD>> FirstOrDefaultAsync<TA, TB, TC, TD>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD>> SingleAsync<TA, TB, TC, TD>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE>> Fetch<TA, TB, TC, TD, TE>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE> FirstOrDefault<TA, TB, TC, TD, TE>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE> Single<TA, TB, TC, TD, TE>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE>> FetchAsync<TA, TB, TC, TD, TE>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE>> FirstOrDefaultAsync<TA, TB, TC, TD, TE>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE>> SingleAsync<TA, TB, TC, TD, TE>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF>> Fetch<TA, TB, TC, TD, TE, TF>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF> FirstOrDefault<TA, TB, TC, TD, TE, TF>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF> Single<TA, TB, TC, TD, TE, TF>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF>> FetchAsync<TA, TB, TC, TD, TE, TF>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF>> SingleAsync<TA, TB, TC, TD, TE, TF>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG>> Fetch<TA, TB, TC, TD, TE, TF, TG>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG> Single<TA, TB, TC, TD, TE, TF, TG>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG>> FetchAsync<TA, TB, TC, TD, TE, TF, TG>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG>> SingleAsync<TA, TB, TC, TD, TE, TF, TG>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH> Single<TA, TB, TC, TD, TE, TF, TG, TH>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public TI RecordI { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H, TI I)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
            this.RecordI = I;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH, TI>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI> Single<TA, TB, TC, TD, TE, TF, TG, TH, TI>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public TI RecordI { get; set; }
        public TJ RecordJ { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H, TI I, TJ J)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
            this.RecordI = I;
            this.RecordJ = J;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ> Single<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public TI RecordI { get; set; }
        public TJ RecordJ { get; set; }
        public TK RecordK { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H, TI I, TJ J, TK K)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
            this.RecordI = I;
            this.RecordJ = J;
            this.RecordK = K;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK> Single<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public TI RecordI { get; set; }
        public TJ RecordJ { get; set; }
        public TK RecordK { get; set; }
        public TL RecordL { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H, TI I, TJ J, TK K, TL L)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
            this.RecordI = I;
            this.RecordJ = J;
            this.RecordK = K;
            this.RecordL = L;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL> Single<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public TI RecordI { get; set; }
        public TJ RecordJ { get; set; }
        public TK RecordK { get; set; }
        public TL RecordL { get; set; }
        public TM RecordM { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H, TI I, TJ J, TK K, TL L, TM M)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
            this.RecordI = I;
            this.RecordJ = J;
            this.RecordK = K;
            this.RecordL = L;
            this.RecordM = M;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL), typeof(TM)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11], (TM)row[12]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM> Single<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL), typeof(TM)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11], (TM)row[12]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public TI RecordI { get; set; }
        public TJ RecordJ { get; set; }
        public TK RecordK { get; set; }
        public TL RecordL { get; set; }
        public TM RecordM { get; set; }
        public TN RecordN { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H, TI I, TJ J, TK K, TL L, TM M, TN N)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
            this.RecordI = I;
            this.RecordJ = J;
            this.RecordK = K;
            this.RecordL = L;
            this.RecordM = M;
            this.RecordN = N;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL), typeof(TM), typeof(TN)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11], (TM)row[12], (TN)row[13]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN> Single<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL), typeof(TM), typeof(TN)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11], (TM)row[12], (TN)row[13]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public TI RecordI { get; set; }
        public TJ RecordJ { get; set; }
        public TK RecordK { get; set; }
        public TL RecordL { get; set; }
        public TM RecordM { get; set; }
        public TN RecordN { get; set; }
        public TO RecordO { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H, TI I, TJ J, TK K, TL L, TM M, TN N, TO O)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
            this.RecordI = I;
            this.RecordJ = J;
            this.RecordK = K;
            this.RecordL = L;
            this.RecordM = M;
            this.RecordN = N;
            this.RecordO = O;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL), typeof(TM), typeof(TN), typeof(TO)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11], (TM)row[12], (TN)row[13], (TO)row[14]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO> Single<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL), typeof(TM), typeof(TN), typeof(TO)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11], (TM)row[12], (TN)row[13], (TO)row[14]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
    public class FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>
    {
        public TA RecordA { get; set; }
        public TB RecordB { get; set; }
        public TC RecordC { get; set; }
        public TD RecordD { get; set; }
        public TE RecordE { get; set; }
        public TF RecordF { get; set; }
        public TG RecordG { get; set; }
        public TH RecordH { get; set; }
        public TI RecordI { get; set; }
        public TJ RecordJ { get; set; }
        public TK RecordK { get; set; }
        public TL RecordL { get; set; }
        public TM RecordM { get; set; }
        public TN RecordN { get; set; }
        public TO RecordO { get; set; }
        public TP RecordP { get; set; }
        public FaaastTuple(TA A, TB B, TC C, TD D, TE E, TF F, TG G, TH H, TI I, TJ J, TK K, TL L, TM M, TN N, TO O, TP P)
        {
            this.RecordA = A;
            this.RecordB = B;
            this.RecordC = C;
            this.RecordD = D;
            this.RecordE = E;
            this.RecordF = F;
            this.RecordG = G;
            this.RecordH = H;
            this.RecordI = I;
            this.RecordJ = J;
            this.RecordK = K;
            this.RecordL = L;
            this.RecordM = M;
            this.RecordN = N;
            this.RecordO = O;
            this.RecordP = P;
        }
    }

    public static partial class DbExtensions
    {
        public static IEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>> Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>(this FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL), typeof(TM), typeof(TN), typeof(TO), typeof(TP)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11], (TM)row[12], (TN)row[13], (TO)row[14], (TP)row[15]);
            }
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP> FirstOrDefault<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>(this FaaastCommand command)
        {
            var enumerator = command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>().GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        public static FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP> Single<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP> result = default;
            foreach (var item in command.Fetch<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>> FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>(this FaaastCommand command)
        {
            await foreach (var row in command.ReadAsync(typeof(TA), typeof(TB), typeof(TC), typeof(TD), typeof(TE), typeof(TF), typeof(TG), typeof(TH), typeof(TI), typeof(TJ), typeof(TK), typeof(TL), typeof(TM), typeof(TN), typeof(TO), typeof(TP)))
            {
                yield return new FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>((TA)row[0], (TB)row[1], (TC)row[2], (TD)row[3], (TE)row[4], (TF)row[5], (TG)row[6], (TH)row[7], (TI)row[8], (TJ)row[9], (TK)row[10], (TL)row[11], (TM)row[12], (TN)row[13], (TO)row[14], (TP)row[15]);
            }
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>> FirstOrDefaultAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>(this FaaastCommand command)
        {
            var enumerator = command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>()
                .GetAsyncEnumerator(command.CancellationToken);
            if(await enumerator.MoveNextAsync())
            {
                return enumerator.Current;
            }

            return default;
        }

        public static async Task<FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>> SingleAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>(this FaaastCommand command)
        {
            int row = 0;
            FaaastTuple<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP> result = default;
            await foreach (var item in command.FetchAsync<TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP>())
            {
                if (row++ > 1)
                    throw new InvalidOperationException("Sequence contains more than one element");

                result = item;
            }

            return result;
        }
    }
}