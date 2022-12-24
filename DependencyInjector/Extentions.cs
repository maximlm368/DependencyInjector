using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    static class ConstructorInfoExtention
    {
        public static bool CanBeFitCtor ( this ConstructorInfo ctor , Type mustNotBeInParams )
        {
            bool isFit = false;

            var ctorParams = ctor . GetParameters ( ) . ToList ( );

            var paramsAreFreeOfCircleReason =
            !ctorParams . ContainsItemWithParticularPart<ParameterInfo , Type> ( mustNotBeInParams , ( item ) => { return item . ParameterType; } );

            if ( paramsAreFreeOfCircleReason )
            {
                isFit = true;
            }

            //bool returnsItSelf = ctor . ReturnType . FullName == ctor . DeclaringType . FullName;

            //if ( ctor . IsStatic    &&    returnsItSelf )
            //{

            //}

            return isFit;
        }
    }



    public static class ListExtention
    {
        public static List<T> Clone<T> ( this List<T> list )
        {
            var result = new List<T> ( );

            for ( var i = 0; i < list . Count; i++ )
            {
                result . Add ( list [ i ] );
            }
            return result;
        }


        public static void AccomplishForEach<T> ( this List<T> list , Action<T> action )
        {
            for ( var j = 0;   j < list . Count;   j++ )
            {
                action ( list [ j ] );
            }
        }


        public static List<T2> GetListOfItemPiece <T1,T2> ( this List<T1> list , Func<T1,T2> partExtracter )
        {
            var result = new List<T2> ( );

            if ( list != null )
            {
                for ( var j = 0;   j < list . Count;   j++ )
                {
                    result . Add ( partExtracter ( list [ j ] ) );
                }
            }
            return result;
        }





        public static bool ContainsItemWithParticularPart <T1,T2> ( this List<T1> list , T2 wantedPart , Func<T1,T2> partExtracter )
        {
            var result = false;

            if ( ( list != null )   &&   ( wantedPart != null )   &&   ( list . Count > 0 ) )
            {
                for ( var j = 0;   j < list . Count;   j++ )
                {
                    if ( wantedPart . Equals ( partExtracter ( list [ j ] ) ) )
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }


        public static IList<bool> PrintMaskOfPresence ( this List<int> list )
        {
            if( list.Count > 0 )
            {
                var arraySize = list . Max ( );
                var array = new bool [ arraySize ];

                for ( var i = 0;    i < list . Count;    i++ )
                {
                    array [ list [ i ] ] = true;
                }

                return array;
            }

            return new bool [ 0 ];
        }


        public static List<int> GetIntersectionWhithMask ( this List<int> list,  IList<bool> mask )
        {
            var intersection = new List<int> ( );

            for ( var i = 0;    i < list . Count;    i++ )
            {
                try
                {
                    if ( mask [ list [ i ] ] )
                    {
                        intersection . Add ( list [ i ] );
                    }
                }
                catch ( ArgumentOutOfRangeException )
                {
                    continue;
                }
            }

            return intersection;
        }


        public static List<List<T>> GroupByDelegateResult <T> ( this List<T> list , Func<T,string> groupingDefiner )
        {
            var result = new List<List<T>> ( );
            var groupNames = new List<string> ( );

            for ( var i = 0;   i < list . Count;   i++ )
            {
                if ( groupNames . Contains ( groupingDefiner ( list [ i ] ) ) )
                {
                    result [ groupNames . IndexOf ( groupingDefiner ( list [ i ] ) ) ] . Add ( list [ i ] );
                }
                else
                {
                    groupNames . Add ( groupingDefiner ( list [ i ] ) );
                    result . Add ( new List<T> ( ) { list [ i ] } );
                }
            }

            return result;
        }


        //public static bool AllItemsCoincideBySomePart <T1, T2> ( this List<T1> list , Func<T1,T2> partDefiner )
        //{
        //    var result = true;

        //    if ( list != null   &&   list . Count > 0 )
        //    {
        //        var somePart = partDefiner ( list [ 0 ] );

        //        for ( var j = 1;   j < list . Count;   j++ )
        //        {
        //            if ( !somePart . Equals ( partDefiner ( list [ j ] ) ) )
        //            {
        //                result = false;
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}


        public static bool AllItemsContainParticularPart <T1, T2> ( this List<T1> list , T2 somePart , Func<T1,T2> partDefiner )
        {
            var result = true;

            if ( ( list != null )   &&   ( somePart != null )   &&   ( list . Count > 0 ) )
            {
                for ( var j = 0;   j < list . Count;   j++ )
                {
                    if ( !somePart . Equals ( partDefiner ( list [ j ] ) ) )
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }


        public static List<T> CutOffSubListByTemplate <T> ( this List<T> list , T cutItem , Func<T,T,bool> comparer )
        {
            var result = new List<T> ( );

            if ( list != null   &&   cutItem != null )
            {
                for ( var j = list . Count - 1;   j >= 0;   j-- )
                {
                    if ( comparer ( list [ j ] , cutItem ) )
                    {
                        list . RemoveAt ( j );
                        result . Add ( list [ j ] );
                    }
                }
            }
            return result;
        }


        public static List<T> GatherRepeated <T> ( this List<T> list )
        {
            var result = new List<T> ( );

            for ( var i = 0;   i < list . Count - 1;   i++ )
            {
                var met = false;

                for ( var j = i + 1;   j < list . Count;   j++ )
                {
                    if ( Object . ReferenceEquals ( list [ i ] , list [ j ] )   &&   result . Contains ( list [ i ] )   &&   result . Contains ( list [ j ] ) )
                    {
                        result . Add ( list [ j ] );
                        met = true;
                        continue;
                    }
                }

                if ( met )
                {
                    continue;
                }
            }

            return result;
        }


        public static bool Cont <T1, T2> ( this List<T1> list , T2 item , Func<T1 , T2 , bool> comparer )
        {
            if ( item == null || list == null )
            {
                return false;
            }

            var result = false;

            if ( list . Count > 0 )
            {
                for ( var i = 0;   i < list . Count;   i++ )
                {
                    if ( comparer ( list [ i ] , item ) )
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }


        //public static List<T> Clone<T> ( this List<T> list )
        //{
        //    var result = new List<T> ( );

        //    for ( var i = 0; i < list . Count; i++ )
        //    {
        //        result . Add ( list [ i ] );
        //    }
        //    return result;
        //}


        public static List<int> GetListAsSiquenceFromZeroToNumber ( int number )
        {
            var result = new List<int> ( );

            for ( var i = 0;   i < number;   i++ )
            {
                result . Add ( i );
            }
            return result;
        }


        //public static List<T> DistinctList<T> ( this List<T> rawList )
        //{
        //    var result = new List<T> ( );

        //    for ( var i = 0; i < rawList . Count; i++ )
        //    {
        //        bool ok = false;

        //        for ( var j = 0; j < result . Count; j++ )
        //        {
        //            if ( rawList [ i ] . Equals ( result [ j ] ) )
        //            {
        //                ok = true;
        //                break;
        //            }
        //        }

        //        if ( !ok )
        //        {
        //            result . Add ( rawList [ i ] );
        //        }
        //    }

        //    return result;
        //}


        public static List<List<T>> MakeMatrixWithDistinctColumn <T> ( this List<List<T>> rawList , int fieldNumber )
        {
            var result = new List<List<T>> ( );

            for ( var i = 0; i < rawList . Count; i++ )
            {
                bool ok = false;

                for ( var j = 0; j < result . Count; j++ )
                {
                    if ( rawList [ i ] [ fieldNumber ] . Equals ( result [ j ] [ fieldNumber ] ) )
                    {
                        ok = true;
                        break; 
                    }
                }

                if ( !ok )
                {
                    result . Add ( rawList [ i ] );
                }
            }
            return result;
        }


        //public static List<List<T>> Convert_List_List_to_List_Array<T> ( this List<T [ ]> la )     // translates List<List<string>> to List<string[]>
        //{
        //    var ll = new List<List<T>> ( );
        //    for ( var i = 0; i < la . Count; i++ )
        //    { var l = new List<T> ( ); for ( var j = 0; j < la [ i ] . Length; j++ ) { l . Add ( la [ i ] [ j ] ); } ll . Add ( l ); }
        //    return ll;
        //}


        //public static List<T> ConvertArrayToList<T> ( T [ ] array )
        //{
        //    var result = new List<T> ( );
            
        //    for ( var i = 0; i < array . Length; i++ )
        //    {
        //        result . Add ( array [ i ] ); 
        //    }
        //    return result;
        //}



        public static List<T> GetColumnFromListOfList<T> ( this List<List<T>> source , int columneNumber )
        {
            var result = new List<T> ( );

            for ( var i = 0;   i < source . Count;   i++ )
            {
                result . Add ( source [ i ] [ columneNumber ] );
            }
            return result;
        }


        public static List<T> Concatinate<T> ( this List<T> receiver , List<T> pushed )
        {
            if ( receiver != null   &&   pushed != null )
            {
                for ( var i = 0;   i < pushed . Count;   i++ )
                {
                    receiver . Add ( pushed [ i ] );
                }
            }
            return receiver;
        }


        public static List<T> SubtractListFromList<T> ( this List<T> more , List<T> less )
        {
            var result = new List<T> ( );
            bool ok = false;

            for ( var i = 0;   i < more . Count;   i++ )
            {
                for ( var j = 0;   j < less . Count;   j++ )
                {
                    if ( less [ j ] . Equals ( more [ i ] ) )
                    {
                        ok = true;
                        break; 
                    }
                }

                if ( ok )
                {
                    ok = false;
                    continue; 
                }
                result . Add ( less [ i ] );
            }
            return result;
        }


    }



    public static class StringExtention
    {
        public static string RidOfSeparator ( this string handledStr , char separator )
        {
            string result = "";

            for ( var j = 0;   j < handledStr . Length;   j++ )
            {
                if ( handledStr [ j ] . Equals ( separator ) )
                {
                    break; 
                }
                else
                {
                    result += handledStr [ j ]; 
                }
            }
            return result;
        }
    }



    public static class DictionaryExtention
    {
        /// <summary>
        /// Returns dictionary with distinct keys each of them has list of values belonging to equal keys from start dictionary
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="startDictionary"></param>
        /// <returns></returns>

        public static Dictionary<T1 , List<T2>> DistinctByKey <T1,T2> ( this Dictionary <T1,T2> startDictionary )
        {
            var result = new Dictionary <T1,List<T2>> ( );

            var distinctKeys = new List<T1> ( );

            if ( startDictionary != null )
            {
                for ( var i = 0;   i < startDictionary . Count;   i++ )
                {
                    if ( result . ContainsKey ( startDictionary . ElementAt ( i ) . Key ) )
                    {
                        result [ startDictionary . ElementAt ( i ) . Key ] . Add ( startDictionary . ElementAt ( i ) . Value );
                    }
                    else
                    {
                        result . Add ( startDictionary . ElementAt ( i ) . Key , new List<T2> ( ) { startDictionary . ElementAt ( i ) . Value } );
                    }
                }
            }
            return result;
        }


        public static List<TKey> ExtractListOfKeys <T,TKey> ( this Dictionary <TKey,T> dictionary )
        {
            var result = new List<TKey> ( );

            if ( dictionary != null )
            {
                var nameArray = new TKey [ dictionary . Count ];
                var keys = dictionary . Keys;
                keys . CopyTo ( nameArray , 0 );
                result = nameArray . ToList ( );
            }

            return result;
        }


    }



    public static class ObjectExtention
    {
        public static string GetCtorParamName ( this Object beingProcessed,  int ctorNumber,  int paramNumber )
        { 
           return beingProcessed . GetType ( ) . GetConstructors ( ) [ ctorNumber ] . GetParameters ( ) [ paramNumber ] . Name;
        }
    }



    public class CircuitListComparer : IEqualityComparer<List<DependencyCircuit>>
    {
        public bool Equals ( List<DependencyCircuit> x , List<DependencyCircuit> y )
        {
            if ( object . ReferenceEquals ( x , y ) )
            {
                return true;
            }

            return false;
        }


        public int GetHashCode ( List<DependencyCircuit> obj )
        {
            return obj . GetHashCode ( );
        }
    }



    class NotInitializedChildException : Exception
    {
    
    }



    class NotBunchedCircuitException : Exception
    {
    
    }

}
