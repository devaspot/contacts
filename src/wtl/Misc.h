#pragma once

ATLINLINE HFONT AtlGetDefaultShellFont()
{
   static CFont s_font;
   if( s_font.IsNull() ) {
      CLogFont lf;
      lf.SetMessageBoxFont();
      s_font.CreateFontIndirect(&lf);
   }
   return s_font;
}
