using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    abstract class NodeType
    {
        public NodeKind _kind;

        public abstract void InitializeNested ( ParamNode becomingInitilized );

        public abstract List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childParamTypes );
    }



    class OrdinaryNode : NodeType
    {
        public NodeKind _kind = NodeKind . Ordinary;

        
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



    class SimpleLeaf : NodeType
    {
        public NodeKind _kind = NodeKind . SimpleLeaf;


        public override void InitializeNested ( ParamNode becomingInitilized )
        {
            becomingInitilized . InitializeSimple ( );
        }


        public override List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childTypes )
        {
            return new List<ParamNode> ( );
        }
    }



    class DependencyCycleParticipant : NodeType
    {
        public NodeKind _kind = NodeKind . DependencyCycleParticipant;

        private bool _timeToGetInitialized = false;


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



    class Fork : DependencyCycleParticipant
    {
        public NodeKind _kind = NodeKind . Fork;

        private bool _timeToGetInitialized = false;



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



    class TopOfLowestInBunch : DependencyCycleParticipant
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



    enum NodeKind
    {
        Ordinary,

        DependencyCycleParticipant,

        SimpleLeaf,

        Fork,

        TopOfLowestInBunch
    }
}