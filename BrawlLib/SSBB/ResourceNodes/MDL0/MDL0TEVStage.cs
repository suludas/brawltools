﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Wii.Graphics;
using BrawlLib.IO;
using BrawlLib.Imaging;
using System.Globalization;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe partial class TEVStageNode : MDL0EntryNode
    {
        public override ResourceType ResourceType { get { return ResourceType.TEVStage; } }
        public override string Name
        {
            get { return String.Format("Stage{0}", Index); }
            set { base.Name = value; }
        }

        public TEVStageNode() { Default(false); }
        public TEVStageNode(ColorEnv colEnv, AlphaEnv alphaEnv, CMD cmd, TevKColorSel kc, TevKAlphaSel ka, TexMapID id, TexCoordID coord, ColorSelChan col, bool useTex) 
        {
            _colorEnv = colEnv;
            _alphaEnv = alphaEnv;
            _cmd = cmd;
            _kcSel = kc;
            _kaSel = ka;
            _texMapID = id;
            _texCoord = coord;
            _colorChan = col;
            _texEnabled = useTex;
        }

        public ColorEnv _colorEnv = new ColorEnv();
        public AlphaEnv _alphaEnv = new AlphaEnv();
        public CMD _cmd = new CMD();
        public TevKColorSel _kcSel;
        public TevKAlphaSel _kaSel;
        public TexMapID _texMapID;
        public TexCoordID _texCoord;
        public ColorSelChan _colorChan;
        public bool _texEnabled;

        //Instead of letting the user copy raw values, there needs to be a way to copy and paste shaders.

        //[Category("f Raw Values")]
        //public string ColorEnv
        //{
        //    get { return "0x" + ((uint)_colorEnv).ToString("X"); }
        //    set
        //    {
        //        if (value.StartsWith("0x"))
        //            value = value.Substring(2, 8);
        //        _colorEnv = uint.Parse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.CurrentCulture);

        //        UpdateProperties();
        //    }
        //}
        //[Category("f Raw Values")]
        //public string AlphaEnv
        //{
        //    get { return "0x" + ((uint)_alphaEnv).ToString("X"); }
        //    set
        //    {
        //        if (value.StartsWith("0x"))
        //            value = value.Substring(2, 8);
        //        _colorEnv = uint.Parse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.CurrentCulture);

        //        UpdateProperties();
        //    }
        //}
        //[Category("f Raw Values")]
        //public string CMD
        //{
        //    get { return "0x" + ((uint)_cmd).ToString("X"); }
        //    set
        //    {
        //        if (value.StartsWith("0x"))
        //            value = value.Substring(2, 8);
        //        _colorEnv = uint.Parse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.CurrentCulture);

        //        UpdateProperties();
        //    }
        //}

        const string eqClr =
@"Shader stages run this equation to blend colors and create the final color of each pixel fragment on a mesh.
Think of it like texture pixels: each pixel has an R, G, B and A channel (red, green, blue and alpha transparency, respectively).

Each shader stage processes color as 3 individual channels: RGB.
This equation is applied to each channel (R, G and B) individually.

Use this equation to predict the output for each channel (resulting in the final color for this stage).
'a' uses input selected by ColorSelectionA.
'b' uses input selected by ColorSelectionB.
'c' uses input selected by ColorSelectionC.
'd' uses input selected by ColorSelectionD.
If clamped, output is restrained to the range [0.0, 1.0].

Note that input is not limited to color values; you can use alpha for all three channels as well.

If you're having a hard time visualizing the output of this equation, think in terms of 0 and 1, where 0 is black and 1 is white.
If the input is 0, nothing multiplied by it passes through. If input is 1, any value multiplied by it will pass through.";

        const string eqAlpha =
@"Shader stages run this equation to blend alpha values and create the final transparency value of each pixel fragment on a mesh.
Think of it like texture pixels: each pixel has an R, G, B and A channel (red, green, blue and alpha transparency, respectively).

Each shader stage processes alpha separately as one channel, 
as you will want to blend transparency differently than color.

Use this equation to predict the output for each channel (resulting in the final color for this stage).
'a' uses input selected by AlphaSelectionA.
'b' uses input selected by AlphaSelectionB.
'c' uses input selected by AlphaSelectionC.
'd' uses input selected by AlphaSelectionD.
If clamped, output is restrained to the range [0.0, 1.0].

Note that input is not limited to alpha; you can use Red, Green or Blue as well (separately).

If you're having a hard time visualizing the output of this equation, think in terms of 0 and 1, where 0 is invisible and 1 is fully visible.
If the input is 0, nothing multiplied by it can affect transparency. If input is 1, any value multiplied by it will affect transparency.";

        //[Category("c TEV Color Env"), Description(eqClr)]
        //public string ColorOutput { get { return (ColorClamp ? "clamp(" : "") + "(d " + (ColorSubtract ? "-" : "+") + " ((1 - c) * a + c * b)" + ((int)ColorBias == 1 ? " + 0.5" : (int)ColorBias == 2 ? " - 0.5" : "") + ") * " + ((int)ColorScale == 3 ? "0.5" : (int)ColorScale == 0 ? "1" : ((int)ColorScale * 2).ToString()) + (ColorClamp ? ");" : ";"); } }
        //[Category("d TEV Alpha Env"), Description(eqAlpha)]
        //public string AlphaOutput { get { return (AlphaClamp ? "clamp(" : "") + "(d " + (AlphaSubtract ? "-" : "+") + " ((1 - c) * a + c * b)" + ((int)AlphaBias == 1 ? " + 0.5" : (int)AlphaBias == 2 ? " - 0.5" : "") + ") * " + ((int)AlphaScale == 3 ? "0.5" : (int)AlphaScale == 0 ? "1" : ((int)AlphaScale * 2).ToString()) + (AlphaClamp ? ");" : ";"); } }

        [Category("c TEV Color Env"), Description(eqClr)]
        public string ColorOutput
        {
            get
            {
                int op = (int)ColorOperation;
                if (op < 2)
                    return (ColorClamp ? "clamp(" : "") + "(d " + (op == 1 ? "-" : "+") + " ((1 - c) * a + c * b) + " + ((int)ColorBias == 1 ? "0.5" : (int)ColorBias == 2 ? "-0.5" : "0") + ") * " + ((int)ColorScale == 3 ? "0.5" : (int)ColorScale == 0 ? "1" : ((int)ColorScale * 2).ToString()) + (ColorClamp ? ");" : ";");
                else if (op > 13)
                    return (ColorClamp ? "clamp(" : "") + "d[x] + ((a[x] " + (op % 2 == 0 ? ">" : "==") + " b[x]) ? c[x] : 0 )" + (ColorClamp ? ");" : ";");
                else
                    return (ColorClamp ? "clamp(" : "") + "d + ((a[" + (op < 10 ? "R" : op < 12 ? "GR" : "BGR") + "] " + (op % 2 == 0 ? ">" : "==") + " b[" + (op < 10 ? "R" : op < 12 ? "GR" : "BGR") + "]) ? c : 0 )" + (ColorClamp ? ");" : ";");
            }
        }
        [Category("d TEV Alpha Env"), Description(eqAlpha)]
        public string AlphaOutput
        {
            get
            {
                int op = (int)AlphaOperation;
                if (op < 2)
                    return (AlphaClamp ? "clamp(" : "") + "(d " + (op == 1 ? "-" : "+") + " ((1 - c) * a + c * b) + " + ((int)AlphaBias == 1 ? "0.5" : (int)AlphaBias == 2 ? "-0.5" : "0") + ") * " + ((int)AlphaScale == 3 ? "0.5" : (int)AlphaScale == 0 ? "1" : ((int)AlphaScale * 2).ToString()) + (AlphaClamp ? ");" : ";");
                else if (op > 13)
                    return (AlphaClamp ? "clamp(" : "") + "d[A] + ((a[A] " + (op % 2 == 0 ? ">" : "==") + " b[A]) ? c[A] : 0 )" + (AlphaClamp ? ");" : ";");
                else
                    return (AlphaClamp ? "clamp(" : "") + "d + ((a[" + (op < 10 ? "R" : op < 12 ? "GR" : "BGR") + "] " + (op % 2 == 0 ? ">" : "==") + " b[" + (op < 10 ? "R" : op < 12 ? "GR" : "BGR") + "]) ? c : 0 )" + (AlphaClamp ? ");" : ";");
            }
        }

        const string konst =
@"Constant1_1 equals 1/1 (1.000).
Constant7_8 equals 7/8 (0.875).
Constant3_4 equals 3/4 (0.750).
Constant5_8 equals 5/8 (0.625).
Constant1_2 equals 1/2 (0.500).
Constant3_8 equals 3/8 (0.375).
Constant1_4 equals 1/4 (0.250).
Constant1_8 equals 1/8 (0.125).";

        const string kColor =
@"This option provides a value to the 'KonstantColorSelection' option in each ColorSelection(A,B,C,D).
You can choose a preset constant or pull a constant value from the material's 'TEV Konstant Block'.
Each color fragment has 4 values: R, G, B, and A. Each shader stage processes color as 3 individual channels: RGB (alpha is separate).

The constants set all 3 channels with the same constant value:
" + konst + @"

The registers ('registers' are just RGBA colors) in the material's  'TEV Konstant Block' can be pulled as well.
You can also swap the channels here, for example, if it says (RGB = GGG), then Red = Green, Blue = Green, and Green = Green.

KSel_0_Value = KReg0Color (RGB = RGB)
KSel_1_Value = KReg1Color (RGB = RGB)
KSel_2_Value = KReg2Color (RGB = RGB)
KSel_3_Value = KReg3Color (RGB = RGB)

KSel_0_Red = KReg0Color (RGB = RRR)
KSel_1_Red = KReg1Color (RGB = RRR)
KSel_2_Red = KReg2Color (RGB = RRR)
KSel_3_Red = KReg3Color (RGB = RRR)

KSel_0_Green = KReg0Color (RGB = GGG)
KSel_1_Green = KReg1Color (RGB = GGG)
KSel_2_Green = KReg2Color (RGB = GGG)
KSel_3_Green = KReg3Color (RGB = GGG)

KSel_0_Blue = KReg0Color (RGB = BBB)
KSel_1_Blue = KReg1Color (RGB = BBB)
KSel_2_Blue = KReg2Color (RGB = BBB)
KSel_3_Blue = KReg3Color (RGB = BBB)

KSel_0_Alpha = KReg0Color (RGB = AAA)
KSel_1_Alpha = KReg1Color (RGB = AAA)
KSel_2_Alpha = KReg2Color (RGB = AAA)
KSel_3_Alpha = KReg3Color (RGB = AAA)";

const string kAlpha =
@"This option provides a value to the 'KonstantColorSelection' option in each ColorSelection(A,B,C,D).
You can choose a preset constant or pull a constant value from the material's 'TEV Konstant Block'.
Each color fragment has 4 values: R, G, B, and A. Each shader stage processes color as 3 individual channels: RGB (alpha is separate).

The constants set all 3 channels with the same constant value:
" + konst + @"


You can also use a different channel as alpha here, for example, for example, if it says (A = R), then the alpha value is set to the value stored in the red channel.

KSel_0_Red = KReg0Color (A = R)
KSel_1_Red = KReg1Color (A = R)
KSel_2_Red = KReg2Color (A = R)
KSel_3_Red = KReg3Color (A = R)

KSel_0_Green = KReg0Color (A = G)
KSel_1_Green = KReg1Color (A = G)
KSel_2_Green = KReg2Color (A = G)
KSel_3_Green = KReg3Color (A = G)

KSel_0_Blue = KReg0Color (A = B)
KSel_1_Blue = KReg1Color (A = B)
KSel_2_Blue = KReg2Color (A = B)
KSel_3_Blue = KReg3Color (A = B)

KSel_0_Alpha = KReg0Color (A = A)
KSel_1_Alpha = KReg1Color (A = A)
KSel_2_Alpha = KReg2Color (A = A)
KSel_3_Alpha = KReg3Color (A = A)";

        [Category("a TEV KSel"), Description(kColor)]
        public TevKColorSel KonstantColorSelection { get { return _kcSel; } set { _kcSel = value; SignalPropertyChange(); } }
        [Category("a TEV KSel"), Description(kAlpha)]
        public TevKAlphaSel KonstantAlphaSelection { get { return _kaSel; } set { _kaSel = value; SignalPropertyChange(); } }

        [Category("b TEV Raster Sources"), Description("This is the index of the texture reference in the material to use as texture input.")]
        public TexMapID TextureMapID { get { return _texMapID; } set { _texMapID = value; SignalPropertyChange(); } }
        [Category("b TEV Raster Sources"), Description("This is the index of the texture coordinate to map the texture on the model.")]
        public TexCoordID TextureCoord { get { return _texCoord; } set { _texCoord = value; SignalPropertyChange(); } }
        [Category("b TEV Raster Sources"), Description(
@"Determines if a texture can be used as color input. 
This stage will grab a pixel fragment from the selected texture reference mapped on the model with the selected coordinates.")]
        public bool TextureEnabled { get { return _texEnabled; } set { _texEnabled = value; SignalPropertyChange(); } }
        [Category("b TEV Raster Sources"), Description(
@"Retrieves a color outputted from the material. This DOES NOT get a color straight from color nodes!
ColorChannel0 retrieves the color from the material's LightChannel0.
ColorChannel1 retrieves the color from the material's LightChannel1.
BumpAlpha retrieves a color from ???
NormalizedBumpAlpha is the same as Bump Alpha, but normalized to the range [0.0, 1.0].
Zero returns a color with all channels set to 0.")]
        public ColorSelChan ColorChannel { get { return _colorChan; } set { _colorChan = value; SignalPropertyChange(); } }

        const string _colorArgDesc =
@"

PreviousColor: returns the RGB of the final output color. This color starts off as 0, 0, 0, 0 (transparent black). This is usually the output of the last TEV stage, unless its ColorRegister property isn't set to 'PreviousRegister'.
PreviousAlpha: returns the alpha value of the final output color for all channels of RGB (RGB = AAA). This color starts off as 0, 0, 0, 0 (transparent black). This is usually the output of the last TEV stage, unless its ColorRegister property isn't set to 'PreviousRegister'.
Color0: returns RGB color of the 1st color register in the attached material's TEV Color Block.
Alpha0: returns the alpha (RGB = AAA) of the 1st color register in the attached material's TEV Color Block.
Color1: returns RGB color of the 2nd color register in the attached material's TEV Color Block.
Alpha1: returns the alpha (RGB = AAA) of the 2nd color register in the attached material's TEV Color Block.
Color2: returns RGB color of the 3rd color register in the attached material's TEV Color Block.
Alpha2: returns the alpha (RGB = AAA) of the 3rd color register in the attached material's TEV Color Block.
TextureColor: returns the RGB color sampled from the texture chosen with TextureMapID, using the coordinates chosen by TextureCoord. Can only be used if TextureEnabled is true.
TextureAlpha: returns the alpha value (RGB = AAA) sampled from the texture chosen with TextureMapID, using the coordinates chosen by TextureCoord. Can only be used if TextureEnabled is true.
RasterColor: returns the RGB color from the source selected by the ColorChannel property.
RasterAlpha: returns the alpha value (RGB = AAA) from the source selected by the ColorChannel property.
One: returns RGB = 1, 1, 1 (white). This color multiplied by another will equal the other color.
Half: returns RGB = 0.5, 0.5, 0.5 (gray). This color multiplied by another will darken the other by half.
KonstantColorSelection: returns the RGB color from the source selected by the KonstantColorSelection property.
Zero: returns RGB = 0, 0, 0 (black). This color multiplied by another will return black.

Note that CLR0 animations can stream RGBA colors into the material's Color and Konstant registers! You can use this property to input new color values into this shader every frame of an animation.";
        
        const string _alphaArgDesc =
@"

PreviousAlpha: returns the alpha value of the final output color. This color starts off as 0, 0, 0, 0 (transparent black). This is usually the output of the last TEV stage, unless its ColorRegister property isn't set to 'PreviousRegister'.
Alpha0: returns the alpha value of the 1st color register in the attached material's TEV Color Block.
Alpha1: returns the alpha value of the 2nd color register in the attached material's TEV Color Block.
Alpha2: returns the alpha value of the 3rd color register in the attached material's TEV Color Block.
TextureAlpha: returns the alpha value sampled from the texture chosen with TextureMapID, using the coordinates chosen by TextureCoord. Can only be used if TextureEnabled is true.
RasterAlpha: returns the alpha value from the source selected by the ColorChannel property.
KonstantAlphaSelection: returns the alpha value from the source selected by the KonstantAlphaSelection property.
Zero: returns 0 (transparent). This alpha multiplied by another will return fully transparent.

Note that CLR0 animations can stream RGBA colors into the material's Color and Konstant registers! You can use this property to input new alpha values into this shader every frame of an animation.";

        [Category("c TEV Color Env"), Description("This is color source for 'a' in the ColorOutput formula." + _colorArgDesc)]
        public ColorArg ColorSelectionA { get { return _colorEnv.SelA; } set { _colorEnv.SelA = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env"), Description("This is color source for 'b' in the ColorOutput formula." + _colorArgDesc)]
        public ColorArg ColorSelectionB { get { return _colorEnv.SelB; } set { _colorEnv.SelB = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env"), Description("This is color source for 'c' in the ColorOutput formula." + _colorArgDesc)]
        public ColorArg ColorSelectionC { get { return _colorEnv.SelC; } set { _colorEnv.SelC = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env"), Description("This is color source for 'd' in the ColorOutput formula." + _colorArgDesc)]
        public ColorArg ColorSelectionD { get { return _colorEnv.SelD; } set { _colorEnv.SelD = value; SignalPropertyChange(); } }

        [Category("c TEV Color Env"), Description("Refer to the ColorOutput property to see effect.")]
        public Bias ColorBias { get { return _colorEnv.Bias; } set { _colorEnv.Bias = value; SignalPropertyChange(); } }
        
        [Category("c TEV Color Env"), Description("Refer to the ColorOutput property to see effect.")]
        public TevOp ColorOperation { get { return _colorEnv.Operation; } set { _colorEnv.Operation = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env"), Description("If true, the final output color channels will be clamped to the range [0, 1].")]
        public bool ColorClamp { get { return _colorEnv.Clamp; } set { _colorEnv.Clamp = value; SignalPropertyChange(); } }

        [Category("c TEV Color Env"), Description("Refer to the ColorOutput property to see effect. This will make the color brighter when multiplying, and darker when dividing.")]
        public TevScale ColorScale { get { return _colorEnv.Shift; } set { _colorEnv.Shift = value; SignalPropertyChange(); } }
        [Category("c TEV Color Env"), Description(
@"This property dictates where the results of the ColorOutput formula should be stored.

PreviousRegister: sets the final output color of this fragment on the model. This is the color that will be seen in-game after the execution of the last shader stage.
Register0: sets the color of the 1st color register in the material's TEV Color Block. This can be used to store a color temporarily for use in one of the ColorSelection properties in a later shader stage.
Register1: sets the color of the 2nd color register in the material's TEV Color Block. This can be used to store a color temporarily for use in one of the ColorSelection properties in a later shader stage.
Register2: sets the color of the 3rd color register in the material's TEV Color Block. This can be used to store a color temporarily for use in one of the ColorSelection properties in a later shader stage.")]
        public TevRegID ColorRegister { get { return _colorEnv.Dest; } set { _colorEnv.Dest = value; SignalPropertyChange(); } }
        
        [Category("d TEV Alpha Env")]
        public TevSwapSel AlphaRasterSwap { get { return _alphaEnv.RSwap; } set { _alphaEnv.RSwap = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Env")]
        public TevSwapSel AlphaTextureSwap { get { return _alphaEnv.TSwap; } set { _alphaEnv.TSwap = value; SignalPropertyChange(); } }

        [Category("d TEV Alpha Output"), Description("This is alpha source for 'a' in the AlphaOutput formula." + _alphaArgDesc)]
        public AlphaArg AlphaSelectionA { get { return _alphaEnv.SelA; } set { _alphaEnv.SelA = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Output"), Description("This is alpha source for 'b' in the AlphaOutput formula." + _alphaArgDesc)]
        public AlphaArg AlphaSelectionB { get { return _alphaEnv.SelB; } set { _alphaEnv.SelB = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Output"), Description("This is alpha source for 'c' in the AlphaOutput formula." + _alphaArgDesc)]
        public AlphaArg AlphaSelectionC { get { return _alphaEnv.SelC; } set { _alphaEnv.SelC = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Output"), Description("This is alpha source for 'd' in the AlphaOutput formula." + _alphaArgDesc)]
        public AlphaArg AlphaSelectionD { get { return _alphaEnv.SelD; } set { _alphaEnv.SelD = value; SignalPropertyChange(); } }

        [Category("d TEV Alpha Output"), Description("Refer to the AlphaOutput property to see effect.")]
        public Bias AlphaBias { get { return _alphaEnv.Bias; } set { _alphaEnv.Bias = value; SignalPropertyChange(); } }

        [Category("d TEV Alpha Output"), Description("Refer to the AlphaOutput property to see effect.")]
        public TevOp AlphaOperation { get { return _alphaEnv.Operation; } set { _alphaEnv.Operation = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Output"), Description("If true, the final output alpha will be clamped to the range [0, 1].")]
        public bool AlphaClamp { get { return _alphaEnv.Clamp; } set { _alphaEnv.Clamp = value; SignalPropertyChange(); } }

        [Category("d TEV Alpha Output"), Description("Refer to the AlphaOutput property to see effect.")]
        public TevScale AlphaScale { get { return _alphaEnv.Shift; } set { _alphaEnv.Shift = value; SignalPropertyChange(); } }
        [Category("d TEV Alpha Output"), Description(
@"This property dictates where the results of the AlphaOutput formula should be stored.

PreviousRegister: sets the final output alpha of this fragment on the model. This is the transparency that will be seen in-game after the execution of the last shader stage.
Register0: sets the alpha of the 1st color register in the material's TEV Color Block. This can be used to store an alpha value temporarily for use in one of the AlphaSelection properties in a later shader stage.
Register1: sets the alpha of the 2nd color register in the material's TEV Color Block. This can be used to store an alpha value temporarily for use in one of the AlphaSelection properties in a later shader stage.
Register2: sets the alpha of the 3rd color register in the material's TEV Color Block. This can be used to store an alpha value temporarily for use in one of the AlphaSelection properties in a later shader stage.")]
        public TevRegID AlphaRegister { get { return _alphaEnv.Dest; } set { _alphaEnv.Dest = value; SignalPropertyChange(); } }
        
        [Category("e TEV Indirect Texturing")]
        public IndTexStageID TexStage { get { return _cmd.StageID; } set { _cmd.StageID = value; SignalPropertyChange(); } }
        [Category("e TEV Indirect Texturing")]
        public IndTexFormat TexFormat { get { return _cmd.Format; } set { _cmd.Format = value; SignalPropertyChange(); } }
        [Category("e TEV Indirect Texturing")]
        public IndTexBiasSel Bias { get { return _cmd.Bias; } set { _cmd.Bias = value; SignalPropertyChange(); } }
        [Category("e TEV Indirect Texturing")]
        public IndTexAlphaSel Alpha { get { return _cmd.Alpha; } set { _cmd.Alpha = value; SignalPropertyChange(); } }
        [Category("e TEV Indirect Texturing")]
        public IndTexMtxID Matrix { get { return _cmd.Matrix; } set { _cmd.Matrix = value; SignalPropertyChange(); } }

        [Category("e TEV Indirect Texturing")]
        public IndTexWrap SWrap { get { return _cmd.SWrap; } set { _cmd.SWrap = value; SignalPropertyChange(); } }
        [Category("e TEV Indirect Texturing")]
        public IndTexWrap TWrap { get { return _cmd.TWrap; } set { _cmd.TWrap = value; SignalPropertyChange(); } }
        [Category("e TEV Indirect Texturing")]
        public bool UsePrevStage { get { return _cmd.UsePrevStage; } set { _cmd.UsePrevStage = value; SignalPropertyChange(); } }
        [Category("e TEV Indirect Texturing")]
        public bool UnmodifiedLOD { get { return _cmd.UnmodifiedLOD; } set { _cmd.UnmodifiedLOD = value; SignalPropertyChange(); } }

        public void Default() { Default(true); }
        public void Default(bool change)
        {
            _alphaEnv.SelA = AlphaArg.Zero;
            _alphaEnv.SelB = AlphaArg.Zero;
            _alphaEnv.SelC = AlphaArg.Zero;
            _alphaEnv.SelD = AlphaArg.Zero;
            _alphaEnv.Bias = Wii.Graphics.Bias.Zero;
            _alphaEnv.Clamp = true;

            _colorEnv.SelA = ColorArg.Zero;
            _colorEnv.SelB = ColorArg.Zero;
            _colorEnv.SelC = ColorArg.Zero;
            _colorEnv.SelD = ColorArg.Zero;
            _colorEnv.Bias = Wii.Graphics.Bias.Zero;
            _colorEnv.Clamp = true;

            _texMapID = TexMapID.TexMap7;
            _texCoord = TexCoordID.TexCoord7;
            _colorChan = ColorSelChan.Zero;

            if (change)
                SignalPropertyChange();
        }

        public void DefaultAsMetal(int texIndex)
        {
            if (Index == 0)
            {
                _colorEnv = 0x28F8AF;
                _alphaEnv = 0x08F2F0;
                KonstantColorSelection = TevKColorSel.KSel_0_Value;
                KonstantAlphaSelection = TevKAlphaSel.KSel_0_Alpha;
                _colorChan = (ColorSelChan)0;
                TextureCoord = TexCoordID.TexCoord0 + texIndex;
                TextureMapID = TexMapID.TexMap0 + texIndex;
                TextureEnabled = true;
            }
            else if (Index == 1)
            {
                _colorEnv = 0x08AFF0;
                _alphaEnv = 0x08FF80;
                KonstantColorSelection = TevKColorSel.KSel_0_Value;
                KonstantAlphaSelection = TevKAlphaSel.KSel_0_Alpha;
                _colorChan = (ColorSelChan)1;
                TextureCoord = TexCoordID.TexCoord7;
                TextureMapID = TexMapID.TexMap7;
                TextureEnabled = false;
            }
            else if (Index == 2)
            {
                _colorEnv = 0x08FEB0;
                _alphaEnv = 0x081FF0;
                KonstantColorSelection = TevKColorSel.KSel_1_Value;
                KonstantAlphaSelection = TevKAlphaSel.KSel_0_Alpha;
                _colorChan = (ColorSelChan)0;
                TextureCoord = TexCoordID.TexCoord7;
                TextureMapID = TexMapID.TexMap7;
                TextureEnabled = false;
            }
            else if (Index == 3)
            {
                _colorEnv = 0x0806EF;
                _alphaEnv = 0x081FF0;
                KonstantColorSelection = TevKColorSel.KSel_0_Value;
                KonstantAlphaSelection = TevKAlphaSel.KSel_0_Alpha;
                _colorChan = (ColorSelChan)7;
                TextureCoord = TexCoordID.TexCoord7;
                TextureMapID = TexMapID.TexMap7;
                TextureEnabled = false;
            }
        }

        //Don't get any strings from this node!
        internal override void GetStrings(StringTable table) { }

        public new void SignalPropertyChange()
        {
            if (Parent != null)
                ((MDL0ShaderNode)Parent)._renderUpdate = true;
            base.SignalPropertyChange();
        }
    }
}