;;by 天华AI研究中心
;;天华效率工具平台-通用工具-标注工具

;Dimension Text Adjust:标注尺寸避让：调整标注的位置，避免重叠
(defun c:THDTA(/ yad-dxf yad-perpt yad-chgent ss n m ent en ang w h l_dat l_mov oldang mov s pt pt1 pt2 l_adj en l_en disang disw dish item item1)
  (defun yad-dxf(en n)
    (if (not (listp en)) (setq en (entget en)))
    (cdr (assoc n en))
  )
  (defun yad-perpt(pt pt1 pt2)
    (inters pt1 pt2 pt (polar pt (+ (angle pt1 pt2) (/ pi 2)) 1200) nil)
  )
  (defun yad-chgent(en n / m val)
    (if (not (listp en)) (setq en (entget en)))
    (foreach itm n
      (setq m (car itm) val (cadr itm))
      (if (assoc m en)
        (setq en (subst (cons m val) (assoc m en) en))
        (setq en (append en (list (cons m val))))
      )
    )
    (entmod en)
  )
  (prompt "\n选择需要自动调整文字位置的一组标注尺寸：")
  (if (setq ss (ssget '((0 . "dimension")(-4 . "<or")(70 . 0)(70 . 1)(70 . 32)(70 . 33)(70 . 128)(70 . 129)(70 . 160)(70 . 161)(-4 . "or>"))))
    (progn
      (vl-cmdf "_undo" "_be")
      (vl-cmdf "_.dimedit" "_h" ss "")
      (setq n -1 m 0)
      (repeat (sslength ss)
        (setq ent (ssname ss (setq n (1+ n))))
        (setq en (yad-dxf (tblsearch "block" (yad-dxf ent 2)) -2))
        (while (/= (yad-dxf (setq en (entnext en)) 0) "MTEXT"))
        (setq ang (yad-dxf en 50) h (yad-dxf en 43) w (+ (/ (yad-dxf en 42) 2) (* 0.2 h)) h (* 0.6 h))
        (setq l_dat (cons (list ent ang w h) l_dat))
        (if (< (/ (yad-dxf ent 42) 2) w)
          (if (= (rem (setq m (1+ m)) 2) 0)
            (setq l_mov (cons (list ent ang w h) l_mov))
            (setq l_mov (append l_mov (list (list ent ang w h))))
          )
        )
      )
      (foreach itm l_mov
        (setq ent (car itm) ang (cadr itm) w (caddr itm) h (cadddr itm) pt (yad-dxf ent 11) oldang (angle pt (yad-perpt pt (setq pt1 (yad-dxf ent 10)) (polar pt1 ang 1200))) mov T)
        (while (and mov (setq s (ssget "_f" (list (setq pt1 (polar (polar pt ang w) (+ ang (/ pi 2)) h)) (setq pt2 (polar pt1 (+ ang pi) (* 2 w))) (setq pt2 (polar pt2 (- ang (/ pi 2)) (* 2 h))) (polar pt2 ang (* 2 w)) pt1)
                                           '((0 . "dimension")(-4 . "<or")(70 . 0)(70 . 1)(70 . 32)(70 . 33)(70 . 128)(70 . 129)(70 . 160)(70 . 161)(-4 . "or>"))
                                )))
          (setq n -1 l_adj nil)
          (repeat (sslength s)
            (setq en (ssname s (setq n (1+ n))))
            (if (and (ssmemb en ss) (not (equal en ent)) (setq l_en (yad-dxf l_dat en)) (equal ang (car l_en) 0.01))
              (progn
                (setq pt1 (yad-perpt (yad-dxf en 11) pt (polar pt ang 1200))
                      disang (angle pt1 pt)
                      disw (- (+ w (cadr l_en)) (distance pt pt1))
                      dish (- (+ h (caddr l_en)) (distance pt (yad-perpt (yad-dxf en 11) pt (polar pt (+ ang (/ pi 2)) 1200))))
                )
                (if (and (> dish 0) (not (equal dish 0 1)))
                  (if (setq item (vl-member-if '(lambda(e) (equal (car e) disang 0.01)) l_adj))
                    (setq item (car item) l_adj (subst (list disang (max disw (cadr item)) (max dish (caddr item))) item l_adj))
                    (setq l_adj (cons (list disang disw dish) l_adj))
                  )
                )
              )
            )
          )
          (cond
            ((not l_adj) (setq mov nil))
            ((and (= (length l_adj) 1) (setq item (car l_adj)) (> (setq disw (cadr item)) 0) (not (equal disw 0 1)) (> (caddr item) 0))
              (if (> (yad-dxf ent 70) 128)
                (progn
                  (setq pt1 (yad-perpt pt (setq pt2 (yad-dxf ent 10)) (polar pt2 ang 1200)))
                  (yad-chgent ent (list (list 11 (setq pt (polar pt (setq disang (angle pt pt1)) (* 2 (+ (distance pt pt1) (if (equal disang oldang 0.01) 0 h)))))) (list 70 (+ 128 (rem (yad-dxf ent 70) 128)))))
                )
                (progn
                  (setq mov nil)
                  (yad-chgent ent (list (list 11 (polar pt (car item) disw)) (list 70 (+ 128 (rem (yad-dxf ent 70) 128)))))
                )
              )
            )
            ((and (= (length l_adj) 2) (setq item (car l_adj) item1 (cadr l_adj))
                  (or (and (> (setq disw (cadr item)) 0) (not (equal disw 0 1)) (> (caddr item) 0))
                      (and (> (setq disw (cadr item1)) 0) (not (equal disw 0 1)) (> (caddr item1) 0))
                  ))
              (if (or (> (yad-dxf ent 70) 128) (and (> (caddr item) 0) (> (caddr item1) 0) (> (setq disw (+ (cadr item) (cadr item1))) 0) (not (equal disw 0 1))))
                (progn
                  (setq pt1 (yad-perpt pt (setq pt2 (yad-dxf ent 10)) (polar pt2 ang 1200)))
                  (if (equal pt pt1 1) (setq disang (- ang (/ pi 2))) (setq disang (angle pt pt1)))
                  (yad-chgent ent (list (list 11 (setq pt (polar pt disang (* 2 (+ (distance pt pt1) (if (equal disang oldang 0.01) 0 h)))))) (list 70 (+ 128 (rem (yad-dxf ent 70) 128)))))
                )
                (progn
                  (setq mov nil)
                  (if (or (< (caddr item) 0) (and (< (setq disw (cadr item)) 0) (not (equal disw 0 1))))
                    (setq item item1)
                  )
                  (yad-chgent ent (list (list 11 (polar pt (car item) (cadr item))) (list 70 (+ 128 (rem (yad-dxf ent 70) 128)))))
                )
              )
            )
            (T (setq mov nil))
          )
        )
      )
      (prompt "\n自动调整完毕！")
      (vl-cmdf "_undo" "_e")
    )
  )
  (princ)
)