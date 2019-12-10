(defun c:THLTR (/ cmd ss1 ss2 tmp)
  (command "_.undo" "_begin")
  (setq cmd (getvar "CMDECHO"))
  (setvar "CMDECHO" 0)
  (setq ss1 (ssadd))
  (or Bgap (setq Bgap 0)) ; default
  (initget 4) ; no negative numbers
  (if (setq tmp (getdist (strcat "\n输入断线距离.<"(rtos Bgap)"> ")))
    (setq Bgap tmp)
  )
  ;;  get objects to break
  (if (and (not (prompt "\n选择下方管线："))
           (setq ss1 (ssget "_:L" '((0 . "LINE,ARC,SPLINE,LWPOLYLINE,POLYLINE,CIRCLE,ELLIPSE"))))
           (not (prompt "\n选择上方管线："))
           (setq ss2 (ssget "_:L" '((0 . "LINE,ARC,SPLINE,LWPOLYLINE,POLYLINE,CIRCLE,ELLIPSE"))))
      )
    (break_with ss2 ss1 nil Bgap) ; ss2break ss1breakwith (flag nil = not to break with self)
  )

  (setvar "CMDECHO" cmd)
  (command "_.undo" "_end")
  (princ)
)

(vl-load-com) (princ)
