;; �컪AI�о�����
;; 2019.10.23 ���£���ԭ���Ļ�����֧�ֶ�ѡ
(defun c:THBKL(/ DIST ENT1 ENT2 X1 X2 X3 X4 Y1 Y2 Y3 Y4 B1 B2 SP INT BASEP1 BASEP2 EDGE1 EDGE2) 
  (setq VAR2 (getvar"cmdecho"))
  (setvar "cmdecho" 1)
  (command "ucs" "")
  
  (princ "\nEnter the distance between:<")
  (prin1 LASTD)
  (princ ">")
  (setq DIST (getdist))
  (if (= DIST nil) 
      (if (= LASTD nil) (setq DIST 100) (setq DIST LASTD))
  )
  
  (prompt "\nSelce the lines below:")
  (if (setq BLOWLINES (ssget "_:L" '((0 . "LINE,LWPolyline"))))
        (progn
              (prompt "\nSelce the lines above:")
              (if (setq ABOVELINES (ssget "_:L" '((0 . "LINE,LWPolyline"))))
                    (progn
                            (setq i 0)
                            (repeat (sslength ABOVELINES)
                                (setq j 0)
                                (setq e2 (ssname ABOVELINES i))
                                (repeat (sslength BLOWLINES)
                                      (setq e1 (ssname BLOWLINES j))
                                      
                                      ;; Highlight
                                      (redraw e1 3)
                                      (redraw e2 3)
                                      
                                      ;; intersection points for the two lines
                                      (setq obj1 (vlax-ename->vla-object e1))
                                      (setq obj2 (vlax-ename->vla-object e2))
                                      (foreach pnt (LM:intersections obj1 obj2 acextendnone)
                                           ;; the trimmed line at the intersection point
                                          (setq LINE (list e1 pnt))
                                           ;; double offset toward two sides (start and end)
                                          (setq BASEP1 (vlax-curve-getStartPoint obj1))
                                          (setq BASEP2 (vlax-curve-getendpoint obj1))
                                          (command "offset" DIST  e2  BASEP1 "")
                                          (setq EDGE1 (entlast))
                                          (command "offset" DIST  e2  BASEP2 "")
                                          (setq EDGE2 (entlast))
                                          (command "trim" EDGE1 EDGE2 "" LINE "")
                                          (command "erase" "p" "")
                                          (setq LASTD DIST)
                                      )
                                      
                                      ;; Unhighlight 
                                      (redraw e1 4)
                                      (redraw e2 4)
                                     
                                      (setq j (1+ j))
                                 )
                                (setq i (1+ i))
                            )
                    )
              )
        )
  )
  
  (setvar "cmdecho" VAR2)
  (command "ucs" "p" )
) 

(vl-load-com) (princ)