;;; 框选图框自动输入版本号、出图日期、设计人
;;; 结构三所 聂云
;;; 1.1版 2013-12-29
;;; 1.22版 2018-08-08
(vl-load-com)
(defun c:THSVM ( / ss ss-list en tkname isold? isH? version date date0 name i versionname
             pt0 ang txtang obj pt1a pt2a pt3a pt4a ptv oriX oriY newY thscale
             L0y L0x L9x L9y Ldesx Ldesy p0 p9 pdes pdes1 pdes2 pdes3 ss1desname
             L1 L2 L3 L4 txtH p01 p02 p1 p2 p3 p4 p5 p6 p7 p8 p10R p11L p10 p11 p12 p12R p13 p13L
             ss1 ss2 ss3 en1 oristr ch_end newstr)
  (ny-cmdla0)
  (setq date0 (menucmd "M=$(edtime,$(getvar,date),YYYY-M-DD)"))
  (princ "\n本程序仅适用天华图框，使用时请确保图框全部处于绘图区域，以搜索相关图框信息.")
 ; (princ (strcat "\n当前版次修改设计人: "
 ;                (if
 ;                  (and (/= "1" (setq name (vlax-ldata-get "dict20131223" "key"))) name)
 ;                  (setq versionname name)
 ;                  "图框第一设计人"
 ;                  )
 ;                " ; 如需修改,请使用命令: checkname" "\n"))
  
  (setq name "名字")
  (setq versionname (ny-getstr-dict "请输入当前版次出图人名字(输入“名字”则默认为第一设计人)，须确保全图框处于cad绘图区域 " name "dict20180802"))
  (setq version (ny-getstr-dict "图纸版本" "A" "dict20131222")
        version (strcase version))
  (setq date (ny-getstr-dict "出图日期" date0 "dict20131221"))
  
  (setq ss (ssget '((0 . "INSERT") (2 . "*_THAPE_*L*"))))
  (if ss
    (progn
      (setvar "osmode" 0)
      (setq i 0)
      (setq ss-list (ny-SS2LST ss))
      ;(setq en (car(setq ss-list (ny-SS2LST ss))))
      (foreach en ss-list
        (progn
          (setq i (1+ i))
          (setq tkname (ny-dxfread 2 en)) ;图框名称
          (if (vl-string-search "Double" tkname)
            (setq isold? "old")  ;老图框
            (setq isold? "new")  ;新图框
            )
          (if (vl-string-search "V_" tkname)
            (setq isH? "V")  ;竖图框
            (setq isH? "H")  ;横图框
            )
          ;(setup)         ;初始设置
          (TKangscale)    ;求图框角度、比例
          (if (vl-string-search "A3" tkname)
            (Txtpo-A3)    ;求文字基准点,A3图框
            (Txtpo-A1)    ;求文字基准点,A2以上图框
            )
          (mktxt)    ;写文字
          );end progn
        (princ (strcat "\n 第 " (rtos i 2 0) " 个图框完成输入. :) \n"))
        );end foreach
      
      )
    )
  (ny-cmdla1)
  (princ)
  )

(setq _pi2       (* pi 0.5)
      _pi4       (* pi 0.25)
      _2pi       (* pi 2.)
      _3pi       (* 1.5 pi))
(defun ny-dtor (dang) (* dang (/ pi 180)))
(defun ny-rtod (rang) (* rang (/ 180 pi)))
(defun ny-ucsAng (/ rel) (ANGLE (trans '(0 0) 1 0) (trans '(1 0) 1 0)))
(defun ny-Wcs2Ucs (pt) (trans pt 0 1))
(defun ny-Ucs2Wcs (pt) (trans pt 1 0))
(defun ny-Wcs2Ucsplst (plst)(mapcar '(lambda(x) (ny-Wcs2Ucs x)) plst))
(defun ny-round (num prec)
  (* prec
     (if (minusp num)
       (fix (- (/ num prec) 0.5))
       (fix (+ (/ num prec) 0.5))
       )
     )
  )
(defun ny-Point3dTo2d(pt)
  (if (= (length pt) 3)
    (reverse (cdr(reverse pt)))
    pt
    )
  )
(defun ny-ss2lst (ss / retu)
  (setq retu (apply 'append (ssnamex ss)))
  (setq retu (vl-remove-if-not '(lambda (x) (= (type x) 'ENAME)) retu))
  )
(defun ny-dxfread (code en)
  (cdr (assoc code (entget en)))
  )
(defun ny-getstr-dict (ny-msg str dict / aaa val)
  (if (not (vlax-ldata-get dict "key"))
    (vlax-ldata-put dict "key" str)
    )
  (setq    aaa (vlax-ldata-get dict "key")
        val  (getstring (strcat "\n" ny-msg "<" (vlax-ldata-get dict "key") ">:"))
        )
  (if (/= val "")
    (vlax-ldata-put dict "key" val)
    (vlax-ldata-put dict "key" aaa)
    )
  )
(defun ny_*error* (msg)
  (if msg
    (progn
      (term_dialog)
      (if olderr (setq *error* olderr))
      (if os (setvar "osmode" os))
      (command "_.undo" "_E")
      )
    )
  (princ)
  )
(defun ny-cmdla0()
  (setvar "cmdecho" 0)
  (command "_.undo" "_BE")
  (setq olderr *error* *error* ny_*error*)
  (setq os (getvar "osmode"))
  )
(defun ny-cmdla1()
  (setvar "osmode" os)
  (setq *error* olderr)
  (command "_.undo" "_E")
  )
(defun ny-1str-Postion-N (String Mode IfNum? / lst I Index Num? ifNum)
  (setq    Num? t
        I 0
        )
  (setq lst (vl-string->list String))
  (cond    ((= Mode 1) (setq lst lst))
        ((= Mode 0) (setq lst (reverse lst)))
        )
  (foreach X lst
    (cond    ((= IfNum? 1) (setq ifNum (member X (vl-string->list "0123456789"))))
      ((= IfNum? 0) (setq ifNum (not (member X (vl-string->list "0123456789")))))
      )
    (if   (and Num? ifNum)
      (setq Num? nil
            Index I
            )
      );end if
    (setq I (1+ I))
    );end foreach
  Index
  );end defun
(defun ny-modent (en tylst / c el )
  (setq el (entget en))  ; el 实体dxf 表
  (foreach n tylst
    (if (setq c (assoc (car n) el))
      (setq el (subst n c el))
      (setq el (append el (list n)))
      )
    )
  (entmod el)
  )

;求相对点函数
(defun ny-polar (p0 x y quadrant ang / p1)
  (cond
    ((= 1 quadrant) (setq p1 (polar (polar p0 ang x) (+ ang _pi2) y)))
    ((= 2 quadrant) (setq p1 (polar (polar p0 (+ ang pi) x) (+ ang _pi2) y)))
    ((= 3 quadrant) (setq p1 (polar (polar p0 (+ ang pi) x) (+ ang (* 1.5 pi)) y)))
    ((= 4 quadrant) (setq p1 (polar (polar p0 ang x) (+ ang (* 1.5 pi)) y)))
    )
  p1
  )

;新横图框用vla-getboundingbox求外框需减去633
;竖图框有一张图是准的,其他图不准,换了一种方式来求点,不管他准不准都行
;如果横框也碰到这种问题,就只有根据图框的绝对长度来写了,这样对加长图框不够通用..
;基点绕ang+pi角度 左移633
;不同的图纸中有时候对 有时候不对,还是直接用oriX求点 繁琐点 但是不会错
;(defun minus-D (ang pt / p1) (setq pt (polar pt (+ ang pi) 633)) )

;求图框角度、比例
(defun TKangscale ( )
  (setq pt0 (ny-Point3dTo2d (ny-dxfread 10 en))) ;图框基点,横竖框不同,转为2d点,避免z向错误
  (setq ang (ny-dxfread 50 en)) ;图框角度,此子函数内横竖图框用各自ang求点
  (setq txtang (ny-rtod (- ang (ny-ucsAng))))    ;文本角度,已考虑ucs
  (if (= isH? "V") (setq txtang (+ txtang 270))) ;竖图框+270
  (setq obj (vlax-ename->vla-object en))
  (vla-getboundingbox obj 'll 'ur)
  (setq pt1a (ny-Point3dTo2d (vlax-safearray->list ll)))     ;左下点,WCS          pt4a   pt2a
  (setq pt2a (ny-Point3dTo2d (vlax-safearray->list ur)))     ;右上点,WCS          pt1a   pt3a
  (setq pt4a (list (car pt1a) (cadr pt2a))) ;左上点,WCS
  (setq pt3a (list (car pt2a) (cadr pt1a))) ;右下点,WCS
  
  
  ;图框尺寸
  ;oriX 主要给横框用,防止vla-getboundingbox求图签处不准出现的bug, 哎..
  ;竖框基点是准的,只需要oriY即可求出
  (cond
    ;  ((vl-string-search "A0L" tkname) (setq oriX  1189 oriY 841)) ;需要置后
    ((vl-string-search "A0L1" tkname) (setq oriX 1486 oriY 841))
    ((vl-string-search "A0L2" tkname) (setq oriX 1635 oriY 841))
    ((vl-string-search "A0L" tkname) (setq oriX  1189 oriY 841))
    ((vl-string-search "A1L1" tkname) (setq oriX 1051 oriY 594))
    ((vl-string-search "A1L2" tkname) (setq oriX 1261 oriY 594))
    ((vl-string-search "A1L4" tkname) (setq oriX 1682 oriY 594))
    ((vl-string-search "A1L5" tkname) (setq oriX 1892 oriY 594))
    ((vl-string-search "A1L" tkname) (setq oriX  841 oriY 594))
    ((vl-string-search "A2L1" tkname) (setq oriX 743 oriY 420))
    ((vl-string-search "A2L2" tkname) (setq oriX 891 oriY 420))
    ((vl-string-search "A2L" tkname) (setq oriX  594 oriY 420))
    ((vl-string-search "A3L1" tkname) (setq oriX 630 oriY 297))
    ((vl-string-search "A3L" tkname) (setq oriX  420 oriY 297))
    )
  
  (if (= isH? "V")
    ;竖框
    ;针对不同角度图框求控制点pt2(右上) pt3(右下)
    (progn
      (setq pt3 pt0)
      (cond
        ;0,360
        (
         (or (equal ang 0 0.001) (equal ang (* 2 pi) 0.001))
         (setq newY (distance pt4a pt2a))     ;图框新宽度
         )
        ;0-90
        (
         (and (> ang 0) (< ang _pi2))
         (setq ptv (inters pt3 (polar pt3 (+ ang _pi2) 100) pt1a pt4a nil))
         (setq newY (/ (distance ptv pt4a) (sin ang)))     ;图框新宽度
         )
        ;90
        (
         (equal ang _pi2 0.001)
         (setq newY (distance pt4a pt1a))     ;图框新宽度
         )
        ;90-180
        (
         (and (> ang _pi2) (< ang pi))
         (setq ptv (inters pt3 (polar pt3 (+ ang _pi2) 100) pt1a pt3a nil))
         (setq newY (/ (distance ptv pt1a) (cos ang)))     ;图框新宽度
         )
        ;180
        (
         (equal ang pi 0.001)
         (setq newY (distance pt3a pt1a))     ;图框新宽度
         )
        ;180-270
        (
         (and (> ang pi) (< ang _3pi))
         (setq ptv (inters pt3 (polar pt3 (+ ang _pi2) 100) pt2a pt3a nil))
         (setq newY (/ (distance ptv pt3a) (sin ang)))     ;图框新宽度
         )
        ;270
        (
         (equal ang _3pi 0.001)
         (setq newY (distance pt3a pt2a))     ;图框新宽度
         )
        ;270-360
        (
         (and (> ang ( * 1.5 pi)) (< ang (* 2 pi)))
         (setq ptv (inters pt3 (polar pt3 (+ ang _pi2) 100) pt2a pt4a nil))
         (setq newY (/ (distance ptv pt2a) (cos ang)))     ;图框新宽度
         )
        (t )
        );end cond
      
      (setq thscale (abs (/ newY oriY)))
      (setq pt2 (polar pt3 ang (* thscale oriY)))
      );end progn
    
    ;横框
    ;针对不同角度图框求控制点pt2(右上) pt3(右下)
    (progn
      (cond
        ;0,360
        (
         (or (equal ang 0 0.001) (equal ang (* 2 pi) 0.001))
         (setq newY (distance pt4a pt1a))     ;图框新宽度
         )
        ;0-90
        (
         (and (> ang 0) (< ang _pi2))
         (setq newY (/ (distance pt0 pt1a) (sin ang)))     ;图框新宽度
         )
        ;90
        (
         (equal ang _pi2 0.001)
         (setq newY (/ (distance pt0 pt1a) (sin ang)))     ;图框新宽度
         )
        ;90-180
        (
         (and (> ang _pi2) (< ang pi))
         (setq newY (/ (distance pt0 pt3a) (cos ang)))     ;图框新宽度
         )
        ;180
        (
         (equal ang pi 0.001)
         (setq newY (/ (distance pt0 pt3a) (cos ang)))     ;图框新宽度
         )
        ;180-270
        (
         (and (> ang pi) (< ang _3pi))
         (setq newY (/ (distance pt0 pt2a) (sin ang)))     ;图框新宽度
         )
        ;270
        (
         (equal ang _3pi 0.001)
         (setq newY (/ (distance pt0 pt2a) (sin ang)))     ;图框新宽度
         )
        ;270-360
        (
         (and (> ang ( * 1.5 pi)) (< ang (* 2 pi)))
         (setq newY (/ (distance pt0 pt4a) (cos ang)))     ;图框新宽度
         )
        (t )
        );end cond
      
      (setq thscale (abs (/ newY oriY)))
      (setq pt3 (polar pt0 ang (* thscale oriX)))
      (setq pt2 (polar pt3 (+ ang _pi2) (* thscale oriY)))
      );end progn
    
    );end if
  );end TKangscale

;求文字基准点,A2以上图框
(defun Txtpo-A1 ( / )
  ;版次不同，文字位置不同
  (cond
    ((or (equal version "A") (equal version "H")) (setq L0y (* thscale 139.5)))
    ((or (equal version "B") (equal version "I")) (setq L0y (* thscale 144.5)))
    ((or (equal version "C") (equal version "J")) (setq L0y (* thscale 149.5)))
    ((or (equal version "D") (equal version "K")) (setq L0y (* thscale 154.5)))
    ((or (equal version "E") (equal version "L")) (setq L0y (* thscale 159.5)))
    ((or (equal version "F") (equal version "M")) (setq L0y (* thscale 164.5)))
    ((or (equal version "G") (equal version "N")) (setq L0y (* thscale 169.5)))
    (t (setq L0y (* thscale 139.5)))
    )
  
  ;新旧图框区分开
  (if
    ;老图框
    (= isold? "old")
    (setq L0x (* thscale 55)     ;版次处日期标准基点
          L9x (* thscale 40)     ;底端日期标准基点
          Ldesy (* thscale 131)  ;设计人标准基点距离右下角点y
          Ldesx (* thscale 70)   ;设计人标准基点距离右下角点x
          )
    ;新图框
    (setq L0x (* thscale 55)       ;版次处日期标准基点
          L9x (* thscale 40)       ;底端日期标准基点
          Ldesy (* thscale 139)    ;设计人标准基点距离右下角点y
          Ldesx (* thscale 70)     ;设计人标准基点距离右下角点x
          )
    );end if
  
  (if (= isH? "V") (setq ang (+ ang _3pi))) ;当为竖图框时加 _3pi 与横图框坐标一致
  (setq L9y (* thscale 22)) ;p9离右下角点y向距离
  (setq p0 (ny-polar pt2 L0x L0y 3 ang))  ;版次处日期中心点
  (setq p9 (ny-polar pt3 L9x L9y 2 ang))  ;底端日期中心点
  
  (setq pdes (ny-polar pt3 Ldesx Ldesy 2 ang))  ;第一设计人角点
  (setq pdes1 (polar pdes (+ ang _pi2) (* thscale 8)))
  (setq pdes2 (polar pdes1 ang (* thscale 20)))  ;用20mm框
  (setq pdes3 (polar pdes2 (+ ang _3pi) (* thscale 8)))
  
  (setq ss1desname (ssget "_cp" (ny-Wcs2Ucsplst (list pdes pdes1 pdes2 pdes3)) (list (cons 0 "*TEXT"))))
  (if (and ss1desname (equal versionname "名字"))
      (setq name (ny-dxfread 1 (car (ny-SS2LST ss1desname))))
      (setq name versionname)
      ) ;输入名字则用输入值，否则用图框中第一设计人名字，需要图框处于绘图区才能搜索到
  
  (setq L1 (* thscale 2.50)
        L2 (* thscale 35.00)
        L3 (* thscale 15.00)
        L4 (* thscale 45.00)
        txtH (* thscale 2.50)
        txtH (ny-round txtH 1)
        ) ;版次框处一些相对距离
  
  (setq p01 (polar p0 (+ ang _pi2) L1)
        p1 (polar p01 (+ ang pi) L2)
        p2 (polar p01 (+ ang pi) L3)
        p3 (polar p01 ang L3)
        p4 (polar p01 ang L4)
        )
  (setq p02 (polar p0 (+ ang _3pi) L1)
        p5 (polar p02 (+ ang pi) L2)
        p6 (polar p02 (+ ang pi) L3)
        p7 (polar p02 ang L3)
        p8 (polar p02 ang L4)
        )
  
  ;底端日期框处一些相对距离
  (setq p10R (ny-polar p9 (* thscale 30) (* thscale 4.0) 1 ang))
  (setq p10 (ny-polar p9 (* thscale 30) (* thscale 4.0) 2 ang))
  (setq p11L (ny-polar p9 (* thscale 30) (* thscale 4.0) 3 ang))
  (setq p11 (ny-polar p9 (* thscale 30) (* thscale 4.0) 4 ang))
  
  ;图纸编号框处一些相对距离
  (setq p12 (ny-polar pt3 (* thscale 70) (* thscale 18) 2 ang))
  (setq p12R (ny-polar pt3 (* thscale 10) (* thscale 18) 2 ang))
  (setq p13 (ny-polar pt3 (* thscale 10) (* thscale 10) 2 ang))
  (setq p13L (ny-polar pt3 (* thscale 70) (* thscale 10) 2 ang))
  
  );end Txtpo-A1

;求文字基准点,A3图框
(defun Txtpo-A3 ( / )
  ;版次不同，文字位置不同
  (cond
    ((or (equal version "A") (equal version "H")) (setq L0y (* thscale 97.92)))
    ((or (equal version "B") (equal version "I")) (setq L0y (* thscale 101.5)))
    ((or (equal version "C") (equal version "J")) (setq L0y (* thscale 105.09)))
    ((or (equal version "D") (equal version "K")) (setq L0y (* thscale 108.68)))
    ((or (equal version "E") (equal version "L")) (setq L0y (* thscale 112.27)))
    ((or (equal version "F") (equal version "M")) (setq L0y (* thscale 115.85)))
    ((or (equal version "G") (equal version "N")) (setq L0y (* thscale 119.44)))
    (t (setq L0y (* thscale 97.92)))
    )
  
  ;新旧图框区分开
  (if
    ;老图框
    (= isold? "old")
    (setq L0x (* thscale 37.29)     ;版次处日期标准基点
          L9x (* thscale 26.53)     ;底端日期标准基点
          Ldesy (* thscale 97.56)   ;设计人标准基点距离右下角点y
          Ldesx (* thscale 48.05)   ;设计人标准基点距离右下角点x
          )
    ;新图框
    (setq L0x (* thscale 37.29)     ;版次处日期标准基点
          L9x (* thscale 26.53)     ;底端日期标准基点
          Ldesy (* thscale 97.56)   ;设计人标准基点距离右下角点y
          Ldesx (* thscale 48.05)   ;设计人标准基点距离右下角点x
          )
    );end if
  
  (if (= isH? "V") (setq ang (+ ang _3pi))) ;当为竖图框时加 _3pi 与横图框坐标一致
  (setq L9y (* thscale 13.61)) ;p9离右下角点y向距离
  (setq p0 (ny-polar pt2 L0x L0y 3 ang))  ;版次处日期中心点
  (setq p9 (ny-polar pt3 L9x L9y 2 ang))  ;底端日期中心点
  
  (setq pdes (ny-polar pt3 Ldesx Ldesy 2 ang))  ;第一设计人角点
  (setq pdes1 (polar pdes (+ ang _pi2) (* thscale 5.74)))
  (setq pdes2 (polar pdes1 ang (* thscale 20)))  ;用20mm框
  (setq pdes3 (polar pdes2 (+ ang _3pi) (* thscale 5.74)))
  
  (setq ss1desname (ssget "_cp" (ny-Wcs2Ucsplst (list pdes pdes1 pdes2 pdes3)) (list (cons 0 "*TEXT"))))
  (if (and ss1desname (equal versionname "名字"))
      (setq name (ny-dxfread 1 (car (ny-SS2LST ss1desname))))
      (setq name versionname)
      ) ;输入名字则用输入值，否则用图框中第一设计人名字，需要图框处于绘图区才能搜索到
  
  (setq L1 (* thscale 1.79)
        L2 (* thscale 25.11)
        L3 (* thscale 10.76)
        L4 (* thscale 32.29)
        txtH (* thscale 2.50)
        txtH (ny-round txtH 1)
        ) ;版次框处一些相对距离
  
  (setq p01 (polar p0 (+ ang _pi2) L1)
        p1 (polar p01 (+ ang pi) L2)
        p2 (polar p01 (+ ang pi) L3)
        p3 (polar p01 ang L3)
        p4 (polar p01 ang L4)
        )
  (setq p02 (polar p0 (+ ang _3pi) L1)
        p5 (polar p02 (+ ang pi) L2)
        p6 (polar p02 (+ ang pi) L3)
        p7 (polar p02 ang L3)
        p8 (polar p02 ang L4)
        )
  
  ;底端日期框处一些相对距离
  (setq p10R (ny-polar p9 (* thscale 21.53) (* thscale 2.87) 1 ang))
  (setq p10 (ny-polar p9 (* thscale 21.53) (* thscale 2.87) 2 ang))
  (setq p11L (ny-polar p9 (* thscale 21.53) (* thscale 2.87) 3 ang))
  (setq p11 (ny-polar p9 (* thscale 21.53) (* thscale 2.87) 4 ang))
  
  ;图纸编号框处一些相对距离
  (setq p12 (ny-polar pt3 (* thscale 48.05) (* thscale 10.74) 2 ang))
  (setq p12R (ny-polar pt3 (* thscale 5) (* thscale 10.74) 2 ang))
  (setq p13 (ny-polar pt3 (* thscale 5) (* thscale 5) 2 ang))
  (setq p13L (ny-polar pt3 (* thscale 48.05) (* thscale 5) 2 ang))
  
  );end Txtpo-A3

;写文字
(defun mktxt ( / )
  ;ucs应为正常x-y逆时针坐标系
  (setq p1 (ny-wcs2ucs p1)
        p2 (ny-wcs2ucs p2)
        p3 (ny-wcs2ucs p3)
        p4 (ny-wcs2ucs p4)
        p5 (ny-wcs2ucs p5)
        p6 (ny-wcs2ucs p6)
        p7 (ny-wcs2ucs p7)
        p8 (ny-wcs2ucs p8)
        p9 (ny-wcs2ucs p9)
        p10 (ny-wcs2ucs p10)
        p10R (ny-wcs2ucs p10R)
        p11L (ny-wcs2ucs p11L)
        p11 (ny-wcs2ucs p11)
        p12 (ny-wcs2ucs p12)
        p12R (ny-wcs2ucs p12R)
        p13 (ny-wcs2ucs p13)
        p13L (ny-wcs2ucs p13L)
        )
  
  ;删除原日期版次设计人信息
  (setq ss1 (ssget "_cp" (list p1 p4 p8 p5) (list (cons 0 "*TEXT"))))
  (if ss1 (command "erase" ss1 ""))
  (setq ss2 (ssget "_cp" (list p10 p10R p11 p11L) (list (cons 0 "*TEXT"))))
  (if ss2 (command "erase" ss2 ""))
  
  ;输入新日期版次设计人信息
  ;(if versionname (setq name versionname)) ;如果手动输入名字,以手动输入为优先
  (command "mtext" "non" p1 "j" "mc" "s" "TH-STYLE1" "r" txtang "h" txtH "non" p6 version "")
  (vla-put-layer (vlax-ename->vla-object (entlast)) "C-SHET-SHET")
  (command "mtext" "non" p2 "j" "mc" "s" "TH-STYLE1" "r" txtang  "h" txtH "non" p7 date "")
  (vla-put-layer (vlax-ename->vla-object (entlast)) "C-SHET-SHET")
  (command "mtext" "non" p3 "j" "mc" "s" "TH-STYLE1" "r" txtang  "h" txtH "non" p8 name "")
  (vla-put-layer (vlax-ename->vla-object (entlast)) "C-SHET-SHET")
  (command "mtext" "non" p10 "j" "mc" "s" "TH-STYLE1" "r" txtang  "h" txtH "non" p11 date "")
  (vla-put-layer (vlax-ename->vla-object (entlast)) "C-SHET-SHET")
  
  ;如果搜索到图纸编号则相应的修改版次
  (if
    (setq ss3 (ssget "_cp" (list p12 p12R p13 p13L) (list (cons 0 "*TEXT"))))
    (progn
      (setq en1 (car (ny-SS2LST ss3)))
      (setq oristr (ny-dxfread 1 en1))
      (setq ch_end (vl-string->list (substr oristr (strlen oristr))))   ;最后一个字符
      (if
        (member ch_end (vl-string->list "0123456789")) ;尾数是否是数字
        (setq newstr (strcat oristr version))
        (setq newstr (strcat (substr oristr 1 (- (strlen oristr) (ny-1str-Postion-N oristr 0 1))) version))
        )
      (ny-modent en1 (list (cons 1 newstr)))
      )
    );end if
  
  );end mktxt




;点示意图
;p1 p2 p01 p3 p4
;      p0  顶上日期中点
;p5 p6 p02 p7 p8
;
;pdes1  pdes1
;pdes   pdes3  第一设计人角点
;
;          p10       p10R
;               p9  下部日期中点
;          p11L      p11
;
;          p12        p12R
;          P13L       p13  ;图纸编号角点
