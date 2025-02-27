﻿namespace Dedge

open System
open System.Threading
open System.Runtime.InteropServices

type VisaLengthOptions =
    | Random = 0
    | Thirteen = 13
    | Sixteen = 16

type VerveLengthOptions =
    | Random = 0
    | Sixteen = 16
    | Nineteen = 19

type From12To19 =
    | Random = 0
    | Twelve = 12
    | Thirteen = 13
    | Fourteen = 14
    | Fifteen = 15
    | Sixteen = 16
    | Seventeen = 17
    | Eightteen = 18
    | Nineteen = 19

type From16To19 = 
    | Random = 0
    | Sixteen = 16
    | Seventeen = 17
    | Eightteen = 18
    | Nineteen = 19

type Cardizer =

    // original snippet by @tpetricek
    // https://stackoverflow.com/a/7792667/1248177
    static member private next =
        let seedGenerator = Random()

        let localGenerator =
            new ThreadLocal<Random>(fun _ ->
                lock
                    seedGenerator
                    (fun _ ->
                        let seed = seedGenerator.Next()
                        Random(seed)))

        fun n -> localGenerator.Value.Next(n)

    /// <summary>Returns a random integer within a given range.</summary>
    /// <param name="low">The (inclusive) low value of the range</param>
    /// <param name="high">The (inclusive) high value of the range</param>
    /// <returns>Random integer within a ginven range</returns>
    static member private NextInRange low high = Cardizer.next (high - low + 1) + low

    static member private GetNumber n =
        let n2 = n * 2
        if n2 > 9 then n2 - 9 else n2

    static member inline private CheckDigit sum = ((sum / 10 + 1) * 10 - sum) % 10

    static member private NextUniquePersonalIdentifiers n =
        { 1 .. n } |> Seq.map (fun _ -> Cardizer.next 10)

    static member private ReverseSum (digits: seq<int>): int =
        digits
        |> Seq.rev
        |> Seq.mapi (fun i n -> i % 2 = 0, n)
        |> Seq.sumBy (fun (isEven, n) ->  if isEven then Cardizer.GetNumber n else n)
        |> Cardizer.CheckDigit

    static member private AppendSum digits =
        seq {
            yield! digits
            yield Cardizer.ReverseSum digits
        }

    static member private GenerateCard (prefixes: seq<int>) cardLen =
        let checksumLen = 1
        let prefixesLen = Seq.length prefixes
        let generateLen = cardLen - prefixesLen - checksumLen

        generateLen
        |> Cardizer.NextUniquePersonalIdentifiers
        |> Seq.toList
        |> Seq.append prefixes
        |> Cardizer.AppendSum
        |> String.Concat

    /// <summary>Returns a random Visa number that is of the given available length.</summary>
    /// <param name="visaLengthOption">Credit card's length (default is randomized between 13 or 16)</param>
    /// <returns>Random Visa number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextVisa"/> method.
    /// <code>
    /// void PrintVisa()
    /// {
    ///    Console.WriteLine(Cardizer.NextVisa()); // randomized between 13 or 16
    ///    Console.WriteLine(Cardizer.NextVisa(VisaLengthOptions.Random)); // randomized between 13 or 16
    ///    Console.WriteLine(Cardizer.NextVisa(VisaLengthOptions.Sixteen));
    /// }
    /// </code>
    /// </example>
    static member NextVisa([<Optional; DefaultParameterValue(VisaLengthOptions.Random)>] visaLengthOption) =
        let length =
            match visaLengthOption with
            | VisaLengthOptions.Random -> if Cardizer.next 2 = 0 then 13 else 16
            | _ -> int visaLengthOption
                

        Cardizer.GenerateCard [ 4 ] length

    /// <summary>Returns a random Verve number that is of the given available length.</summary>
    /// <param name="verveLengthOption">Credit card's length (default is randomized between 16 or 19)</param>
    /// <returns>Random Verve number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextVerve"/> method.
    /// <code>
    /// void PrintVerve()
    /// {
    ///    Console.WriteLine(Cardizer.NextVerve()); // randomized between 16 or 19
    ///    Console.WriteLine(Cardizer.NextVerve(VerveLengthOptions.Random)); // randomized between 16 or 19
    ///    Console.WriteLine(Cardizer.NextVerve(VerveLengthOptions.Sixteen));
    /// }
    /// </code>
    /// </example>
    static member NextVerve([<Optional; DefaultParameterValue(VerveLengthOptions.Random)>] verveLengthOption) =
        let numberToSeq number =
            let rec loop n list =
                if n <= 0 then
                    list
                else
                    loop (n / 10) (n % 10 :: list)

            loop number []

        let length =
            match verveLengthOption with
            | VerveLengthOptions.Random -> 16 + 3 * Cardizer.next 2
            | _ -> int verveLengthOption

        let prefix =
            [ [ 506099; 506198 ]
              [ 650002; 650027 ] ].[Cardizer.next 2]

        let prefixes =
            Cardizer.NextInRange prefix.[0] prefix.[1]
            |> numberToSeq

        Cardizer.GenerateCard prefixes length

    /// <summary>Returns a random Mir number that is of the given available length.</summary>
    /// <param name="mirLengthOption">Credit card's length (default is randomized between 16 and 19)</param>
    /// <returns>Random Mir number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextMir"/> method.
    /// <code>
    /// void PrintMir()
    /// {
    ///    Console.WriteLine(Cardizer.NextMir()); // randomized between 16 and 19
    ///    Console.WriteLine(Cardizer.NextMir(MirLengthOptions.Random)); // randomized between 16 and 19
    ///    Console.WriteLine(Cardizer.NextMir(MirLengthOptions.Sixteen));
    /// }
    /// </code>
    /// </example>
    static member NextMir([<Optional; DefaultParameterValue(From16To19.Random)>] mirLengthOption) =
        let length =
            match mirLengthOption with
            | From16To19.Random -> Cardizer.NextInRange 16 19
            | _ -> int mirLengthOption

        let prefixes = [ 2; 2; 0; Cardizer.next 5 ]

        Cardizer.GenerateCard prefixes length

    /// <summary>Returns a random Jcb number that is of the given available length.</summary>
    /// <param name="jcbLengthOption">Credit card's length (default is randomized between 16 and 19)</param>
    /// <returns>Random Jcb number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextJcb"/> method.
    /// <code>
    /// void PrintJcb()
    /// {
    ///    Console.WriteLine(Cardizer.NextJcb()); // randomized between 16 and 19
    ///    Console.WriteLine(Cardizer.NextJcb(JcbLengthOptions.Random)); // randomized between 16 and 19
    ///    Console.WriteLine(Cardizer.NextJcb(JcbLengthOptions.Sixteen));
    /// }
    /// </code>
    /// </example>
    static member NextJcb([<Optional; DefaultParameterValue(From16To19.Random)>] jcbLengthOption) =
        let length =
            match jcbLengthOption with
            | From16To19.Random -> Cardizer.NextInRange 16 19
            | _ -> int jcbLengthOption

        let prefixes =
            [ 3
              5
              Cardizer.NextInRange 2 8
              Cardizer.NextInRange 8 9 ]

        Cardizer.GenerateCard prefixes length

    /// <summary>Returns a random Amex number.</summary>
    /// <returns>Random Amex number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextAmex"/> method.
    /// <code>
    /// void PrintAmex()
    /// {
    ///    Console.WriteLine(Cardizer.NextAmex());
    /// }
    /// </code>
    /// </example>
    static member NextAmex() =
        let second = if Cardizer.next 2 = 0 then 4 else 7
        Cardizer.GenerateCard [3; second] 15

    /// <summary>Returns a random Discover number that is of the given available length.</summary>
    /// <param name="discoverLengthOption">Credit card's length (default is randomized between 16 and 19)</param>
    /// <returns>Random Discover number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextDiscover"/> method.
    /// <code>
    /// void PrintDiscover()
    /// {
    ///    Console.WriteLine(Cardizer.NextDiscover()); // randomized between 16 and 19
    ///    Console.WriteLine(Cardizer.NextDiscover(DiscoverLengthOptions.Random)); // randomized between 16 and 19
    ///    Console.WriteLine(Cardizer.NextDiscover(DiscoverLengthOptions.Sixteen));
    /// }
    /// </code>
    /// </example>
    static member NextDiscover([<Optional; DefaultParameterValue(From16To19.Random)>] discoverLengthOption) =
        let length =
            match discoverLengthOption with
            | From16To19.Random -> Cardizer.NextInRange 16 19
            | _ -> int discoverLengthOption

        Cardizer.GenerateCard [ 6; 0; 1; 1 ] length

    /// <summary>Returns a random MasterCard number.</summary>
    /// <returns>Random MasterCard number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextMasterCard"/> method.
    /// <code>
    /// void PrintMasterCard()
    /// {
    ///    Console.WriteLine(Cardizer.NextMasterCard());
    /// }
    /// </code>
    /// </example>
    static member NextMasterCard() =
        let second = Cardizer.next 4 + 1
        Cardizer.GenerateCard [ 5; second ] 16


    /// <summary>Returns a random Uatp number.</summary>
    /// <returns>Random Uatp number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextUatp"/> method.
    /// <code>
    /// void PrintUatp()
    /// {
    ///    Console.WriteLine(Cardizer.NextUatp());
    /// }
    /// </code>
    /// </example>
    static member NextUatp () =
        Cardizer.GenerateCard [1] 15

    /// <summary>Returns a random Maestro number.</summary>
    /// <returns>Random Maestro number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextMaestro"/> method.
    /// <code>
    /// void PrintMaestro()
    /// {
    ///    Console.WriteLine(Cardizer.NextMaestro());
    /// }
    /// </code>
    /// </example>
    static member NextMaestro([<Optional; DefaultParameterValue(From12To19.Random)>] maestroLengthOption) =
        let length =
            match maestroLengthOption with
            | From12To19.Random -> Cardizer.NextInRange 12 19
            | _ -> int maestroLengthOption

        let prefix =
            [
                [ 5; 0; 1; 8 ]
                [ 5; 0; 2; 0 ] 
                [ 5; 0; 3; 8 ]
                [ 5; 8; 9; 3 ]
                [ 6; 3; 0; 4 ]
                [ 6; 7; 5; 9 ]
                [ 6; 7; 6; 1 ]
                [ 6; 7; 6; 2 ]
                [ 6; 7; 6; 3 ]].[Cardizer.next 9]

        Cardizer.GenerateCard prefix length
    
    /// <summary>Returns a random Dankort number.</summary>
    /// <returns>Random Dankort number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextDankort"/> method.
    /// <code>
    /// void PrintDankort()
    /// {
    ///    Console.WriteLine(Cardizer.NextDankort());
    /// }
    /// </code>
    /// </example>
    static member NextDankort([<Optional; DefaultParameterValue(true)>]  acceptCoBranded: bool) =
        let prefixDankort = [ 5; 0; 1; 9 ]
        let prefixVisaCobranded = [ 4; 5; 7; 1 ]
        let prefix =
            if acceptCoBranded
            then
                [ prefixDankort
                  prefixVisaCobranded ].[Cardizer.next 2]
            else prefixDankort
         
        Cardizer.GenerateCard prefix 16 

    /// <summary>Returns a random InterPayment number.</summary>
    /// <returns>Random InterPayment number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextInterPayment"/> method.
    /// <code>
    /// void PrintInterPayment()
    /// {
    ///    Console.WriteLine(Cardizer.NextInterPayment());
    /// }
    /// </code>
    /// </example>
    static member NextInterPayment([<Optional; DefaultParameterValue(From16To19.Random)>] interPaymentLengthOption) =
        let length =
            match interPaymentLengthOption with
            | From16To19.Random -> Cardizer.NextInRange 16 19
            | _ -> int interPaymentLengthOption

        Cardizer.GenerateCard [ 6; 3; 6 ] length

    /// <summary>Returns a random UnionPay number.</summary>
    /// <returns>Random UnionPay number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextUnionPay"/> method.
    /// <code>
    /// void PrintUnionPay()
    /// {
    ///    Console.WriteLine(Cardizer.NextUnionPay());
    /// }
    /// </code>
    /// </example>
    static member NextUnionPay([<Optional; DefaultParameterValue(From16To19.Random)>] unionPayLengthOption) =
        let length =
            match unionPayLengthOption with
            | From16To19.Random -> Cardizer.NextInRange 16 19
            | _ -> int unionPayLengthOption

        Cardizer.GenerateCard [ 6; 2 ] length

    /// <summary>Returns a random Tunion number.</summary>
    /// <returns>Random Tunion number</returns>
    /// <example>
    /// This sample shows how to call the <see cref="NextTunion"/> method.
    /// <code>
    /// void PrintTunion()
    /// {
    ///    Console.WriteLine(Cardizer.NextTunion());
    /// }
    /// </code>
    /// </example>
    static member NextTunion() =
        Cardizer.GenerateCard [ 3; 1 ] 19
