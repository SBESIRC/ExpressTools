;; 天华AI研究中心
;; 2019.10.23 更新：在原来的基础上支持多选
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
  (if (setq BLOWLINES (ssget '((0 . "LINE"))))
        (progn
              (prompt "\nSelce the lines above:")
              (if (setq ABOVELINES (ssget '((0 . "LINE"))))
                    (progn
                            (setq i 0)
                            (repeat (sslength ABOVELINES)
                                (setq j 0)
                                (repeat (sslength BLOWLINES)
                                      (setq e1 (ssname BLOWLINES j))
                                      (setq e2 (ssname ABOVELINES i))
                                      
                                      ;; Highlight
                                      (redraw e1 3)
                                      (redraw e2 3)
                                      
                                      (setq ENT1 (entget e1))
                                      (setq ENT2 (entget e2))
                                      (setq X1 (nth 1 (assoc 10 ENT1)))
                                      (setq Y1 (nth 2 (assoc 10 ENT1)))
                                      (setq X2 (nth 1 (assoc 11 ENT1)))
                                      (setq Y2 (nth 2 (assoc 11 ENT1)))
                                      (setq X3 (nth 1 (assoc 10 ENT2)))
                                      (setq Y3 (nth 2 (assoc 10 ENT2)))
                                      (setq X4 (nth 1 (assoc 11 ENT2)))
                                      (setq Y4 (nth 2 (assoc 11 ENT2)))
                                      (if (and (= X1 X2) (/= X3 X4)) 
                                           (progn
                                            (setq X X1)
                                            (setq B2 (/ (- Y4 Y3) (- X4 X3)))
                                            (setq Y (+ Y4 (* B2 (- X1 X4))))
                                           )
                                      )
                                      (if (and (/= X1 X2) (= X3 X4))
                                           (progn
                                            (setq X X3)
                                            (setq B1 (/ (- Y2 Y1) (- X2 X1)))
                                            (setq Y (+ Y1 (* B1 (- X3 X1))))
                                           )
                                         
                                      )
                                      (if (and (/= X1 X2) (/= X3 X4))
                                           (progn
                                            (setq B1 (/ (- Y2 Y1) (- X2 X1)))
                                            (setq B2 (/ (- Y4 Y3) (- X4 X3)))
                                            (setq X (/ (- (+ (* B2 X4) Y2) (+ (* B1 X2) Y4)) (- B2 B1)))
                                            (setq Y (+ Y4 (* B2 (- X X4))))
                                           )

                                      )                         
                                      
                                      ;; intersection points for the two lines
                                      (foreach pnt (LM:intersections (vlax-ename->vla-object e1) (vlax-ename->vla-object e2) acextendnone)
                                           ;; the trimmed line at the intersection point
                                          (setq LINE (list e1 pnt))
                                        
                                          (setq BASEP1 (list X1 Y1 0))
                                          (setq BASEP2 (list X2 Y2 0))
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
; All rights reserved by <Andre Studio 2003>