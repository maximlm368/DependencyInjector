using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    public abstract class NodeType
    {
        public NodeKind _kind;

        public abstract void InitializeNested ( ParamNode becomingInitilized );

        // public abstract List<ParamNode> DefineChildren ( ParamNode becomingParent ,  out List<DependencyChain> dependencyChains );

        public abstract List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childParamTypes );
    }



    public class OrdinaryNode : NodeType
    {
        public NodeKind _kind = NodeKind . Ordinary;
        private static int _idCounter;
        private int _id;


        public OrdinaryNode ( )
        {
            _id = _idCounter;
            _idCounter++;
        }


        public override bool Equals ( object obj )
        {
            bool equals = false;
            OrdinaryNode beingCompared = null;

            if ( obj is OrdinaryNode )
            {
                beingCompared = ( OrdinaryNode ) obj;

                if ( beingCompared . _id == _id )
                {
                    equals = true;
                }
            }
            return equals;
        }


        public override void InitializeNested ( ParamNode becomingInitilized )
        {
            becomingInitilized . InitializeOrdinary ( );
        }


        public override List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childTypes )
        {
            var resultChildren = new List<ParamNode> ( );

            for ( var counter = 0;   counter < childTypes . Count;   counter++ )
            {
                var nodeType = DefineNodeType ( childTypes [ counter ] );
                var child = new ParamNode ( childTypes [ counter ] , counter , becomingParent , nodeType );
                resultChildren . Add ( child );
            }
            return resultChildren;
        }


        private NodeType DefineNodeType ( Type paramType )
        {
            NodeType result;
            bool isSimple = ( paramType . IsPrimitive )   ||   ( paramType . FullName == "System.String" );

            if ( isSimple )
            {
                result = new SimpleLeaf ( );
            }
            else
            {
                result = new OrdinaryNode ( );
            }
            return result;
        }
    }



    public class SimpleLeaf : NodeType
    {
        public NodeKind _kind = NodeKind . SimpleLeaf;
        private static int _idCounter;
        private int _id;


        public SimpleLeaf ( )
        {
            _id = _idCounter;
            _idCounter++;
        }


        public override bool Equals ( object obj )
        {
            bool equals = false;

            SimpleLeaf beingCompared = null;

            if ( obj is SimpleLeaf )
            {
                beingCompared = ( SimpleLeaf ) obj;

                if ( beingCompared . _id == _id )
                {
                    equals = true;
                }
            }

            return equals;
        }


        public override void InitializeNested ( ParamNode becomingInitilized )
        {
            becomingInitilized . InitializeSimple ( );
        }


        public override List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childTypes )
        {
            return new List<ParamNode> ( );
        }
    }



    public class DependencyCycleParticipant : NodeType
    {
        public NodeKind _kind = NodeKind . DependencyCycleParticipant;
        private static int _idCounter;
        private int _id;
        private bool _timeToGetInitialized = false;


        public DependencyCycleParticipant ( )
        {
            _id = _idCounter;
            _idCounter++;
        }


        public override bool Equals ( object obj )
        {
            bool equals = false;
            DependencyCycleParticipant beingCompared = null;

            if ( obj is DependencyCycleParticipant )
            {
                beingCompared = ( DependencyCycleParticipant ) obj;

                if ( beingCompared . _id == _id )
                {
                    equals = true;
                }
            }
            return equals;
        }


        public override void InitializeNested ( ParamNode becomingInitilized )
        {
            if ( _timeToGetInitialized )
            {
                becomingInitilized . InitializeOrdinary ( );
            }
            else
            {
                _timeToGetInitialized = true;
                return;
            }
        }


        public override List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childParamTypes )
        {
            return new List<ParamNode> ( );
        }
    }



    public class Fork : DependencyCycleParticipant
    {
        public NodeKind _kind = NodeKind . Fork;
        
        private static int _idCounter;
        
        private int _id;

        private bool _timeToGetInitialized = false;


        public override bool Equals ( object obj )
        {
            bool equals = false;
            Fork beingCompared = null;

            if ( obj is Fork )
            {
                beingCompared = ( Fork ) obj;

                if ( beingCompared . _id == _id )
                {
                    equals = true;
                }
            }
            return equals;
        }


        //public override void InitializeNested ( ParamNode becomingInitilized )
        //{
        //    if( _timeToGetInitialized )
        //    {
        //        becomingInitilized . InitializeOrdinary ( );
        //    }
        //    else
        //    {
        //        _timeToGetInitialized = true;
        //        return;
        //    }
        //}


        //public override List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childParamTypes )
        //{
        //    return new List<ParamNode> ( );
        //}
    }



    public class TopOfLowestInBunch : DependencyCycleParticipant
    {
        public override List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childParamTypes )
        {
            throw new NotImplementedException ( );
        }

        public override void InitializeNested ( ParamNode becomingInitilized )
        {
            throw new NotImplementedException ( );
        }
    }



    public enum NodeKind
    {
        Ordinary,

        DependencyCycleParticipant,

        SimpleLeaf,

        Fork,

        TopOfLowestInBunch
    }
}