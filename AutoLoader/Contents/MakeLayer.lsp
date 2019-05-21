(defun MakeLayer ( name colour linetype lineweight willplot bitflag description )
 ;; Lee Mac 2010
 (or (tblsearch "LAYER" name)
   (entmake
     (append
       (list
         (cons 0 "LAYER")
         (cons 100 "AcDbSymbolTableRecord")
         (cons 100 "AcDbLayerTableRecord")
         (cons 2  name)
         (cons 70 bitflag)
         (cons 290 (if willplot 1 0))
         (cons 6
           (if (and linetype (tblsearch "LTYPE" linetype))
             linetype "CONTINUOUS"
           )
         )
         (cons 62 (if (and colour (< 0 (abs colour) 256)) colour 7))
         (cons 370
           (if (minusp lineweight) -3
             (fix
               (* 100
                 (if (and lineweight (<= 0.0 lineweight 2.11)) lineweight 0.0)
               )
             )
           )
         )
       )
       (if description
         (list
           (list -3
             (list "AcAecLayerStandard" (cons 1000 "") (cons 1000 description))
           )
         )
       )
     )
   )
 )
)

(defun c:norlays nil (vl-load-com)
 
 ;; Lee Mac 2010
 ;; Specifications:
 ;; Description        Data Type        Remarks
 ;; -----------------------------------------------------------------
 ;; Layer Name          STRING          Only standard chars allowed
 ;; Layer Colour        INTEGER         may be nil, -ve for Layer Off, Colour < 256
 ;; Layer Linetype      STRING          may be nil, If not loaded, CONTINUOUS.
 ;; Layer Lineweight    REAL            may be nil, negative=Default, otherwise 0 <= x <= 2.11
 ;; Plot?               BOOLEAN         T = Plot Layer, nil otherwise
 ;; Bit Flag            INTEGER         0=None, 1=Frozen, 2=Frozen in VP, 4=Locked
 ;; Description         STRING          may be nil for no description
 ;; Function will return list detailing whether layer creation is successful.    
 (
   (lambda ( lst / lts ) (setq lts (vla-get-Linetypes (vla-get-ActiveDocument (vlax-get-acad-object))))
     (mapcar 'cons (mapcar 'car lst)
       (mapcar
         (function
           (lambda ( x )
             (and (caddr x)
               (or (tblsearch "LTYPE" (caddr x))
                 (vl-catch-all-apply 'vla-load (list lts (caddr x) "acad.lin"))
               )
             )
             (apply 'MakeLayer x)
           )
         )
         lst
       )
     )
   )
  '(
   ;  Name                 Colour   Linetype    Lineweight Plot? Bitflag  Description 
   ( "EL_COMPONENTS"            7  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_CONSTR_COMP"           1  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_FITTINGS"              4  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_LABEL"                 1  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_LABEL_TXT"           252  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_TERMINALS"             1  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_TERMINALS_PE"         24  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_TERMINALS_TXT"       252  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_TXT"                 252  "CONTINUOUS"     -3       T      0      nil  )
   ( "EL_WIRES"               252  "CONTINUOUS"     -3       T      0      nil  )
   ( "GE_ANNOTATION"            7  "CONTINUOUS"     -3       T      0      nil  )
   ( "GE_LABEL"                10  "CONTINUOUS"     -3       T      0      nil  )
   ( "GE_LABEL_TEXT"          253  "CONTINUOUS"     -3       T      0      nil  )
   ( "GE_TXT_LANGUAGE_DU"     252  "CONTINUOUS"     -3       T      0      nil  )
   ( "GE_TXT_LANGUAGE_EN"     252  "CONTINUOUS"     -3       T      0      nil  )
   ( "GE_TXT_LANGUAGE_FR"     252  "CONTINUOUS"     -3       T      0      nil  )
   ( "GE_TXT_LANGUAGE_GE"     252  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_HEADER_FRAME"          7  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_HEADER_TXT"            7  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_MATLIST"             254  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_MATLIST_FRAME"       254  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_MATLIST_POS"         254  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_MATLIST_TXT"         252  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_TITLE_FRAME"           7  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_TITLE_LOGO"           10  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_TITLE_LOGO_TXT"        7  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_TITLE_TXT"             7  "CONTINUOUS"     -3       T      0      nil  )
   ( "LA_VIEWPORTS"           230  "CONTINUOUS"     -3      nil     0      nil  )
   ( "PN_ACCESSORIES"          30  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_ACTUATORS"           160  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_AIR_LINE_EQUIPMENT"   40  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_BRACKET_MOUNTING"      1  "ACAD_ISO12W100" -3       T      0      nil  )
   ( "PN_CABINET"               8  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_CABINET_DIM"           8  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_COMPONENTS"            7  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_CONDUCTS"              3  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_CONSTR_COMP"           2  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_DRAIN"                 3  "HIDDEN"         -3       T      0      nil  )
   ( "PN_EXHAUST"             104  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_FITTINGS"             30  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_IDENTIFICATION"        4  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_PILOT_SUPPLY"          3  "HIDDEN"         -3       T      0      nil  )
   ( "PN_PORT_NUMBERS"          8  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_PRESSURE_SWITCHES"   200  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_PROPORTIONAL_VALVES" 226  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_SECTIONS"              3  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_SUB-BASES"             8  "ACAD_ISO12W100" -3       T      0      nil  )
   ( "PN_SUB-BASE_CONDUCTS"     2  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_SUPPLY"                3  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_TXT"                   3  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_VACUUM"               60  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_VALVES"              240  "CONTINUOUS"     -3       T      0      nil  )
   ( "PN_VALVES_OVERRIDE"       1  "CONTINUOUS"     -3       T      0      nil  )
   )
 )
)
